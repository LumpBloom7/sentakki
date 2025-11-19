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
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

public partial class SentakkiBeatmapConverter
{
    private SentakkiHitObject convertSlider(HitObject original) => convertSlider(original, currentLane, false, true);

    private SentakkiHitObject convertSlider(HitObject original, int lane, bool forceHoldNote, bool allowFans)
    {
        double duration = ((IHasDuration)original).Duration;

        var slider = (IHasPathWithRepeats)original;

        bool isSuitableSlider = !isLazySlider(original);

        if (isSuitableSlider && !forceHoldNote)
        {
            var slide = tryConvertToSlide(original, lane, allowFans);

            if (slide is not null)
                return slide;
        }

        bool isBreak = slider.NodeSamples[0].Any(s => s.Name == HitSampleInfo.HIT_FINISH);

        var hold = new Hold
        {
            Lane = lane,
            Break = isBreak,
            StartTime = original.StartTime,
            Duration = duration,
            Samples = slider.NodeSamples[0],
        };

        return hold;
    }

    private Slide? tryConvertToSlide(HitObject original, int lane, bool allowFans)
    {
        var nodeSamples = ((IHasPathWithRepeats)original).NodeSamples;

        bool tailBreak = nodeSamples.Last().Any(s => s.Name == HitSampleInfo.HIT_FINISH);
        bool headBreak = nodeSamples.First().Any(s => s.Name == HitSampleInfo.HIT_FINISH);

        double beatLength = Beatmap.ControlPointInfo.TimingPointAt(original.StartTime).BeatLength;
        double duration = ((IHasDuration)original).Duration;

        float waitDurationBeats = 1;

        // This is an attempt to make shoot delays more appropriate for the slide duration
        while (waitDurationBeats * beatLength >= duration - 50)
        {
            waitDurationBeats /= 2;
            // If wait duration is below 0.25 beats, then this cannot ever be a slide
            if (waitDurationBeats < 0.25)
                return null;
        }

        double waitDurationMs = waitDurationBeats * beatLength;

        var selectedPath = chooseSlidePartFor(original, allowFans, duration - waitDurationMs);

        if (selectedPath is null)
            return null;

        var slide = new Slide
        {
            SlideInfoList =
            [
                new SlideBodyInfo
                {
                    Segments = selectedPath,
                    Duration = ((IHasDuration)original).Duration,
                    Break = tailBreak,
                    WaitDuration = waitDurationMs,
                }
            ],
            Lane = lane.NormalizeLane(),
            StartTime = original.StartTime,
            Samples = nodeSamples.FirstOrDefault(),
            Break = headBreak,
        };

        return slide;
    }

    private IReadOnlyList<SlideSegment>? chooseSlidePartFor(HitObject original, bool allowFans, double duration)
    {
        double velocity = original is IHasSliderVelocity slider ? slider.SliderVelocityMultiplier * beatmap.Difficulty.SliderMultiplier : 1;
        double adjustedDuration = duration * velocity;

        var candidates = SlidePaths.VALID_CONVERT_PATHS.AsEnumerable();
        if (!ConversionFlags.HasFlag(ConversionFlags.FanSlides) || !allowFans)
            candidates = candidates.Where(p => p.Segment.Shape != PathShapes.Fan);

        if (!ConversionFlags.HasFlag(ConversionFlags.DisableCompositeSlides))
        {
            List<SlideSegment> parts = [];

            double durationLeft = duration;

            SlideSegment? lastPart = null;

            double velocityAdjustmentFactor = 1 + 0.5 / velocity;

            while (true)
            {
                var nextChoices = candidates.Where(p => p.MinDuration * velocityAdjustmentFactor < durationLeft)
                                            .Shuffle(rng)
                                            .SkipWhile(p => p.Segment.Shape == PathShapes.Circle && !isValidCircleComposition(p.Segment, lastPart));

                if (!nextChoices.Any())
                    break;

                var chosen = nextChoices.First();

                durationLeft -= chosen.MinDuration * velocityAdjustmentFactor;
                parts.Add((lastPart = chosen.Segment).Value);
            }

            if (parts.Count == 0)
                return null;

            return [.. parts];
        }
        else
        {
            // Find the part that is the closest
            return
            [
                candidates.GroupBy(t => getDelta(t.MinDuration))
                          .OrderBy(g => g.Key)
                          .Take(5)
                          .ProbabilityPick(t => t.Key, rng)
                          .Shuffle(rng)
                          .First()
                          .Segment
            ];
        }

        double getDelta(double d)
        {
            double diff = adjustedDuration - d * 2;
            if (diff > 0) diff *= 3; // We don't want to overly favor longer slides when a shorter one is available

            return Math.Round(Math.Abs(diff) * 0.02) * 100; // Round to nearest 100ms
        }
    }

    private static bool isValidCircleComposition(SlideSegment part, SlideSegment? previousPart)
    {
        if (previousPart is null)
            return true;

        if (previousPart.Value.Shape != PathShapes.Circle)
            return true;

        if (part.Mirrored != previousPart.Value.Mirrored)
            return true;

        return previousPart.Value.RelativeEndLane == 0;
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

        double sliderVelocity = hitObject is IHasSliderVelocity sv ? sv.SliderVelocityMultiplier : DifficultyControlPoint.DEFAULT.SliderVelocity;
        double scoringDistance = 100 * difficulty.SliderMultiplier * sliderVelocity;
        double velocity = scoringDistance / timingPoint.BeatLength;
        double tickDistance = scoringDistance / difficulty.SliderTickRate;

        var sliderEvents = SliderEventGenerator.Generate(hitObject.StartTime, spanDuration, velocity, tickDistance, slider.Path.Distance, slider.RepeatCount + 1, CancellationToken.None);

        var sliderOrigin = slider.Path.PositionAt(0);

        // Check if any events such as ticks or repeats are a certain distance from the origin, requiring a cursor move.
        return sliderEvents.All(e => !((slider.Path.PositionAt(e.PathProgress) - sliderOrigin).LengthSquared >= distanceCutoffSquared));
    }
}
