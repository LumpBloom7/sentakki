using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

public partial class NewBeatmapConverter
{
    private static readonly Vector2 standard_playfield_size = new Vector2(512, 384);
    private static readonly Vector2 standard_playfield_center = standard_playfield_size / 2;

    private readonly ConversionExperiments conversionExperiments;

    private int currentLane;

    private readonly double timePreempt;
    private readonly float circleRadius;

    private readonly IBeatmap beatmap;

    private readonly SentakkiPatternGenerator patternGenerator;

    private RotationDirection? activeStreamDirection;

    private double stackThreshold => timePreempt * beatmap.BeatmapInfo.StackLeniency;

    public NewBeatmapConverter(IBeatmap beatmap, ConversionExperiments experiments)
    {
        this.beatmap = beatmap;

        conversionExperiments = experiments;

        // Taking this from osu specific information that we need
        timePreempt = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.ApproachRate, 1800, 1200, 450);
        circleRadius = 54.4f - 4.48f * beatmap.Difficulty.CircleSize;

        patternGenerator = new SentakkiPatternGenerator(beatmap);

        if (beatmap.HitObjects.Count == 0)
        {
            currentLane = 0;
            return;
        }

        float angle = standard_playfield_center.GetDegreesFromPosition(beatmap.HitObjects[0].GetPosition());

        currentLane = getClosestLaneFor(angle);
    }

    public IEnumerable<SentakkiHitObject> ConvertHitObject(HitObject original)
    {
        HitObject? previous = null;
        HitObject? next = null;

        for (int i = 0; i < beatmap.HitObjects.Count; ++i)
        {
            if (beatmap.HitObjects[i] != original) continue;

            if (i > 0 && isChronologicallyClose(beatmap.HitObjects[i - 1], original))
                previous = beatmap.HitObjects[i - 1];

            if (i < beatmap.HitObjects.Count - 1 && isChronologicallyClose(original, beatmap.HitObjects[i + 1]))
                next = beatmap.HitObjects[i + 1];

            break;
        }

        SentakkiHitObject result = original switch
        {
            IHasPathWithRepeats => convertSlider(original),
            IHasDuration => convertSpinner(original),
            _ => convertHitCircle(original)
        };

        // Set up the next hitobject position
        bool isJump = NewBeatmapConverter.isJump(original, next);
        bool isStack = this.isStack(original, next);
        bool isStream = this.isStream(original, next);

        if (isJump || isStack)
        {
            activeStreamDirection = null;

            if (isJump)
                currentLane += jumpLaneOffset(original, previous, next);
        }
        else if (isStream)
        {
            Debug.Assert(next is not null);

            bool isSpacedStream = isOverlapping(original, next);
            activeStreamDirection = getStreamDirection(original, previous, next);
            int streamOffset = activeStreamDirection == RotationDirection.Clockwise ? 1 : -1;

            // Slides have special behavior
            if (result is Slide slide)
            {
                if (!isStack && isStream)
                    currentLane = slide.Lane + slide.SlideInfoList[0].SlidePath.EndLane;

                if (isSpacedStream)
                    currentLane += streamOffset;
            }
            else
            {
                currentLane += streamOffset;
            }
        }
        else
        {
            float nextAngle = 0;
            if (next is not null)
                nextAngle = standard_playfield_center.GetDegreesFromPosition(next.GetPosition());

            currentLane = getClosestLaneFor(nextAngle);
        }

        currentLane = currentLane.NormalizePath();

        yield return result;
    }

    private bool isStack(HitObject original, HitObject? next)
    {
        const int stack_distance_squared = 9;

        if (next is null)
            return false;

        if (next.StartTime - original.GetEndTime() >= stackThreshold)
            return false;

        bool isWithinStdStackDistance = Vector2Extensions.DistanceSquared(original.GetPosition(), next.GetPosition()) < stack_distance_squared;

        return isWithinStdStackDistance;
    }

    private bool isStream(HitObject original, HitObject? next)
    {
        if (next is null)
            return false;

        double timeDelta = next.StartTime - original.GetEndTime();
        double beatLength = beatmap.ControlPointInfo.TimingPointAt(next.StartTime).BeatLength;

        double quarterBeatLength = beatLength * 0.25f;

        bool snappedToQuarterBeat = timeDelta <= quarterBeatLength || MathHelper.ApproximatelyEquivalent(timeDelta, quarterBeatLength, 0.1);

        return isOverlapping(original, next) || snappedToQuarterBeat;
    }

    private bool isOverlapping(HitObject original, HitObject next)
        => (next.GetPosition() - original.GetPosition()).LengthSquared <= Math.Pow(circleRadius * 2, 2);

    private static bool isJump(HitObject original, HitObject? next)
    {
        // If two notes are this distance apart, consider it a jump
        const float jump_distance_threshold_squared = 128 * 128;

        if (next is null)
            return false;

        float distanceSqr = Vector2.DistanceSquared(original.GetEndPosition(), next.GetPosition());

        return distanceSqr >= jump_distance_threshold_squared;
    }

    private static int jumpLaneOffset(HitObject original, HitObject? previous, HitObject? next)
    {
        if (next is null)
            return 0;

        Vector2 midpoint = midpointOf(original, previous, next);

        float angle1 = midpoint.GetDegreesFromPosition(original.GetEndPosition());
        float angle2 = midpoint.GetDegreesFromPosition(next.GetPosition());

        return getClosestLaneFor(angle2) - getClosestLaneFor(angle1);
    }

    private RotationDirection getStreamDirection(HitObject original, HitObject? previous, HitObject? next)
    {
        if (next is null)
            return activeStreamDirection ?? RotationDirection.Clockwise;

        var midpoint = midpointOf(original, previous, next);

        float currAngle = midpoint.GetDegreesFromPosition(original.GetEndPosition());
        float nextAngle = midpoint.GetDegreesFromPosition(next.GetPosition());

        float angleDelta = SentakkiExtensions.GetDeltaAngle(currAngle, nextAngle);

        if (angleDelta == 0)
            return activeStreamDirection ?? RotationDirection.Clockwise;

        return angleDelta > 0 ? RotationDirection.Clockwise : RotationDirection.Counterclockwise;
    }

    private static Vector2 midpointOf(HitObject current, HitObject? previous, HitObject? next)
    {
        // Use the playfield center as a fallback if we don't have the previous note
        Vector2 midpoint = standard_playfield_center;

        int pointCount = 1;
        Vector2 pointSum = current.GetEndPosition();

        if (previous is not null)
        {
            pointSum += previous.GetEndPosition();
            pointCount++;
        }

        if (next is not null)
        {
            pointSum += next.GetPosition();
            pointCount++;
        }

        // If the current hitobject has a different start and end position, we can use that as even more info
        if (!current.GetPosition().Equals(current.GetEndPosition()))
        {
            pointSum += current.GetPosition();
            pointCount++;
        }

        if (pointCount > 2)
            midpoint = pointSum / pointCount;

        return midpoint;
    }

    private static int getClosestLaneFor(float angle)
    {
        angle = angle.NormalizeAngle();

        float minDelta = float.MaxValue;
        int closestLane = -1;

        for (int i = 0; i < 8; ++i)
        {
            float delta = Math.Abs(SentakkiExtensions.GetDeltaAngle(i.GetRotationForLane(), angle));

            if (!(delta < minDelta)) continue;

            closestLane = i;
            minDelta = delta;
        }

        return closestLane;
    }

    private bool isChronologicallyClose(HitObject a, HitObject b)
    {
        double timeDelta = b.StartTime - a.GetEndTime();
        double beatLength = beatmap.ControlPointInfo.TimingPointAt(b.StartTime).BeatLength;

        return timeDelta < beatLength || MathHelper.ApproximatelyEquivalent(timeDelta, beatLength, 0.1);
    }
}
