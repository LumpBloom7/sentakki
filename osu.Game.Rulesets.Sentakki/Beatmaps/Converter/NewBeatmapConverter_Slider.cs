using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

public partial class NewBeatmapConverter
{
    private SentakkiHitObject convertSlider(HitObject original, HitObject? previous, HitObject? next)
    {
        double duration = ((IHasDuration)original).Duration;

        bool stacked = isStack(original, previous);
        bool inStream = isStream(original, previous);

        var slider = (IHasPathWithRepeats)original;

        bool isBreak = slider.NodeSamples[0].Any(s => s.Name == HitSampleInfo.HIT_FINISH);
        bool isSpecial = slider.NodeSamples[0].Any(s => s.Name == HitSampleInfo.HIT_WHISTLE);

        if (stacked || !inStream)
            streamDirection = null;
        else
            streamDirection = getStreamDirection(original, previous, next);

        int streamOffset = streamDirection == RotationDirection.Clockwise ? 1 : -1;

        int lane = stacked
            ? currentLane
            : inStream
                ? currentLane + streamOffset
                : patternGenerator.RNG.Next(8);

        if (isSpecial)
        {
            var slide = tryConvertToSlide(original, lane);

            if (slide is not null)
            {
                currentLane = slide.Value.endLane;
                return slide.Value.Item1;
            }
        }

        return new Hold
        {
            Lane = currentLane = lane.NormalizePath(),
            Break = isBreak,
            StartTime = original.StartTime,
            Duration = duration,
            NodeSamples = slider.NodeSamples,
        };
    }

    private (Slide, int endLane)? tryConvertToSlide(HitObject original, int lane)
    {
        var nodeSamples = ((IHasPathWithRepeats)original).NodeSamples;

        var selectedPath = chooseSlidePartFor(original);

        if (selectedPath is null)
            return null;

        bool tailBreak = nodeSamples.Last().Any(s => s.Name == HitSampleInfo.HIT_FINISH);
        bool headBreak = nodeSamples.First().Any(s => s.Name == HitSampleInfo.HIT_FINISH);

        int end = lane + selectedPath.EndOffset;

        var slide = new Slide
        {
            SlideInfoList = new List<SlideBodyInfo>()
            {
                new SlideBodyInfo
                {
                    SlidePathParts = new[] { selectedPath },
                    Duration = ((IHasDuration)original).Duration,
                    Break = tailBreak,
                    ShootDelay = 0.5f,
                }
            },
            Lane = lane.NormalizePath(),
            StartTime = original.StartTime,
            NodeSamples = nodeSamples,
            Break = headBreak
        };

        return (slide, end);
    }

    private SlideBodyPart? chooseSlidePartFor(HitObject original)
    {
        double velocity = original is IHasSliderVelocity slider ? slider.SliderVelocity : 1;
        double duration = ((IHasDuration)original).Duration;
        double adjustedDuration = duration * velocity;

        var candidates = SlidePaths.VALIDPATHS;
        if (!conversionExperiments.HasFlag(ConversionExperiments.fanSlides))
            candidates = candidates.Where(p => p.SlidePart.Shape != SlidePaths.PathShapes.Fan).ToList();

        // Find the part that is the closest
        return candidates.GroupBy(t => getDelta(t.MinDuration))
                         .OrderBy(g => g.Key)
                         .Take(5)
                         .ProbabilityPick(t => t.Key, patternGenerator.RNG)
                         .Shuffle(patternGenerator.RNG)
                         .First()
                         .SlidePart;

        double getDelta(double d)
        {
            double diff = adjustedDuration - d * 2;
            if (diff > 0) diff *= 1.5; // We don't want to overly favor longer slides when a shorter one is available

            return Math.Round(Math.Abs(diff) * 0.02) * 50; // Round to nearest 50ms
        }
    }
}
