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

public partial class SentakkiBeatmapConverter
{
    private SentakkiHitObject convertSlider(HitObject original)
    {
        double duration = ((IHasDuration)original).Duration;

        var slider = (IHasPathWithRepeats)original;

        bool isSuitableSlider = !isLazySlider(original);

        bool isBreak = slider.NodeSamples[0].Any(s => s.Name == HitSampleInfo.HIT_FINISH);

        if (isSuitableSlider)
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

        int endOffset = selectedPath.Sum(p => p.EndOffset);

        int end = lane + endOffset;

        var slide = new Slide
        {
            SlideInfoList = new List<SlideBodyInfo>()
            {
                new SlideBodyInfo
                {
                    SlidePathParts = selectedPath,
                    Duration = ((IHasDuration)original).Duration,
                    Break = tailBreak,
                    ShootDelay = 0.5f,
                }
            },
            Lane = lane.NormalizePath(),
            StartTime = original.StartTime,
            Samples = nodeSamples.FirstOrDefault(),
            Break = headBreak
        };

        return (slide, end);
    }

    private SlideBodyPart[]? chooseSlidePartFor(HitObject original)
    {
        double velocity = original is IHasSliderVelocity slider ? (slider.SliderVelocityMultiplier * beatmap.Difficulty.SliderMultiplier) : 1;
        double duration = ((IHasDuration)original).Duration;
        double adjustedDuration = duration * velocity;

        var candidates = SlidePaths.VALIDPATHS.AsEnumerable();
        if (!ConversionFlags.HasFlag(ConversionFlags.fanSlides))
            candidates = candidates.Where(p => p.SlidePart.Shape != SlidePaths.PathShapes.Fan);

        if (!ConversionFlags.HasFlag(ConversionFlags.disableCompositeSlides))
        {
            List<SlideBodyPart> parts = new List<SlideBodyPart>();

            double durationLeft = duration;

            SlideBodyPart? lastPart = null;

            double velocityAdjustmentFactor = 1 + (0.5 / velocity);

            while (true)
            {
                var nextChoices = candidates.Where(p => p.MinDuration * velocityAdjustmentFactor < durationLeft)
                    .Shuffle(rng)
                    .SkipWhile(p => p.SlidePart.Shape == SlidePaths.PathShapes.Circle && !isValidCircleComposition(p.SlidePart, lastPart));

                if (!nextChoices.Any())
                    break;

                var chosen = nextChoices.First();

                durationLeft -= chosen.MinDuration * velocityAdjustmentFactor;
                parts.Add((lastPart = chosen.SlidePart).Value);
            }

            if (!parts.Any())
                return null;

            return parts.ToArray();
        }
        else
        {
            // Find the part that is the closest
            return new[]
            {
                candidates.GroupBy(t => getDelta(t.MinDuration))
                          .OrderBy(g => g.Key)
                          .Take(5)
                          .ProbabilityPick(t => t.Key, rng)
                          .Shuffle(rng)
                          .First()
                          .SlidePart
            };
        }

        double getDelta(double d)
        {
            double diff = adjustedDuration - (d * 2);
            if (diff > 0) diff *= 3; // We don't want to overly favor longer slides when a shorter one is available

            return Math.Round(Math.Abs(diff) * 0.02) * 100; // Round to nearest 100ms
        }
    }

    private static bool isValidCircleComposition(SlideBodyPart part, SlideBodyPart? previousPart)
    {
        if (previousPart is null)
            return true;

        if (previousPart.Value.Shape != SlidePaths.PathShapes.Circle)
            return true;

        if (part.Mirrored != previousPart.Value.Mirrored)
            return true;

        return previousPart.Value.EndOffset == 0;
    }

    // This checks whether a slider can be completed without moving the mouse at all.
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

        double sliderVelocity = (hitObject is IHasSliderVelocity sv) ? sv.SliderVelocityMultiplier : DifficultyControlPoint.DEFAULT.SliderVelocity;
        double scoringDistance = 100 * difficulty.SliderMultiplier * sliderVelocity;
        double velocity = scoringDistance / timingPoint.BeatLength;
        double tickDistance = scoringDistance / difficulty.SliderTickRate;

        var sliderEvents = SliderEventGenerator.Generate(hitObject.StartTime, spanDuration, velocity, tickDistance, slider.Path.Distance, slider.RepeatCount + 1, CancellationToken.None);

        var sliderOrigin = slider.Path.PositionAt(0);

        // Check if any events such as ticks or repeats are a certain distance from the origin, requiring a cursor move.
        return sliderEvents.All(e => !((slider.Path.PositionAt(e.PathProgress) - sliderOrigin).LengthSquared >= distanceCutoffSquared));
    }
}
