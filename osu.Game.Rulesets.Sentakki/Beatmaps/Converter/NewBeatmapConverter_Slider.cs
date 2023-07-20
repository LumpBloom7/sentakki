using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

public partial class NewBeatmapConverter
{
    private SentakkiHitObject convertSlider(HitObject original)
    {
        double duration = ((IHasDuration)original).Duration;

        var slider = (IHasPathWithRepeats)original;

        bool isSuitableSlider = !isLazySlider(original);

        bool isBreak = slider.NodeSamples[0].Any(s => s.Name == HitSampleInfo.HIT_FINISH);
        bool isSpecial = isSuitableSlider
                         && (!conversionExperiments.HasFlag(ConversionExperiments.restoreSlideHitWhistle)
                             || slider.NodeSamples[0].Any(s => s.Name == HitSampleInfo.HIT_WHISTLE));

        if (isSpecial)
        {
            var slide = tryConvertToSlide(original, currentLane);

            if (slide is not null)
                return slide.Value.Item1;
        }

        var hold = new Hold
        {
            Lane = currentLane = currentLane.NormalizePath(),
            Break = isBreak,
            StartTime = original.StartTime,
            Duration = duration,
            NodeSamples = slider.NodeSamples,
        };
        return hold;
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
        double velocity = original is IHasSliderVelocity slider ? (slider.SliderVelocity * beatmap.Difficulty.SliderMultiplier) : 1;
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

    private bool isLazySlider(HitObject hitObject)
    {
        const float follow_radius_scale = 2.4f;

        if (hitObject is not IHasPathWithRepeats slider)
            return false;

        float distanceCutoffSquared = MathF.Pow(circleRadius * follow_radius_scale, 2);

        double spanDuration = slider.Duration / (slider.RepeatCount + 1);
        var difficulty = beatmap.BeatmapInfo.Difficulty;

        var controlPointInfo = (LegacyControlPointInfo)beatmap.ControlPointInfo;

        TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(hitObject.StartTime);

        double sliderVelocity = (hitObject is IHasSliderVelocity sv) ? sv.SliderVelocity : DifficultyControlPoint.DEFAULT.SliderVelocity;
        double scoringDistance = 100 * difficulty.SliderMultiplier * sliderVelocity;
        double velocity = scoringDistance / timingPoint.BeatLength;
        double tickDistance = scoringDistance / difficulty.SliderTickRate;
        double legacyLastTickOffset = (hitObject as IHasLegacyLastTickOffset)?.LegacyLastTickOffset ?? 0;

        var sliderEvents = SliderEventGenerator.Generate(hitObject.StartTime, spanDuration, velocity, tickDistance, slider.Path.Distance, slider.RepeatCount + 1, legacyLastTickOffset,
            CancellationToken.None);

        var sliderOrigin = slider.Path.PositionAt(0);

        Console.WriteLine($"{sliderOrigin}, {slider.Path.Distance}");

        foreach (var e in sliderEvents)
        {
            switch (e.Type)
            {
                case SliderEventType.Repeat:
                case SliderEventType.Tail:
                case SliderEventType.Tick:
                case SliderEventType.LegacyLastTick:
                    if ((slider.Path.PositionAt(e.PathProgress) - sliderOrigin).LengthSquared > distanceCutoffSquared)
                        return false;

                    break;

                default:
                    continue;
            }
        }

        return true;
    }
}
