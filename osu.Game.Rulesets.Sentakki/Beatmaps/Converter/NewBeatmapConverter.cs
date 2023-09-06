using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Extensions.IEnumerableExtensions;
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

    // osu beatmap specific details
    private readonly double timePreempt;
    private readonly float circleRadius;

    private readonly double stackThreshold;

    private readonly IBeatmap beatmap;

    // Current converter state
    private StreamDirection activeStreamDirection;
    private int currentLane;
    private readonly Random rng;

    public NewBeatmapConverter(IBeatmap beatmap, ConversionExperiments experiments)
    {
        this.beatmap = beatmap;

        conversionExperiments = experiments;

        // Taking this from osu specific information that we need
        timePreempt = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.ApproachRate, 1800, 1200, 450);
        circleRadius = 54.4f - 4.48f * beatmap.Difficulty.CircleSize;
        stackThreshold = timePreempt * beatmap.BeatmapInfo.StackLeniency;

        // Prep an RNG with a seed generated from beatmap diff
        var difficulty = beatmap.BeatmapInfo.Difficulty;
        int seed = ((int)MathF.Round(difficulty.DrainRate + difficulty.CircleSize) * 20) + (int)(difficulty.OverallDifficulty * 41.2) + (int)MathF.Round(difficulty.ApproachRate);
        rng = new Random(seed);

        if (beatmap.HitObjects.Count == 0)
            return;

        float angle = standard_playfield_center.GetDegreesFromPosition(beatmap.HitObjects[0].GetPosition());
        currentLane = getClosestLaneFor(angle);
    }

    public IEnumerable<SentakkiHitObject> ConvertHitObject(HitObject original)
    {
        SentakkiHitObject result = original switch
        {
            IHasPathWithRepeats => convertSlider(original),
            IHasDuration => convertSpinner(original),
            _ => convertHitCircle(original)
        };

        // Update the lane to be used by the next hitobject
        updateCurrentLane(original, result);

        yield return result;
    }

    private void updateCurrentLane(HitObject original, SentakkiHitObject converted)
    {
        HitObject? next = beatmap.HitObjects.GetNext(original);

        // Check if next note even exists
        if (next is null)
            return;

        // If the next note is far off, we start from a fresh slate
        if (!isChronologicallyClose(original, next))
        {
            float nextAngle = standard_playfield_center.GetDegreesFromPosition(next.GetPosition());
            currentLane = getClosestLaneFor(nextAngle).NormalizePath();
            activeStreamDirection = StreamDirection.None;
            return;
        }

        var previous = beatmap.HitObjects.GetPrevious(original);

        // If the jumps are huge, directly use angle differences to replicate the distance
        if (isJump(original, next))
        {
            activeStreamDirection = StreamDirection.None;
            currentLane = (currentLane + jumpLaneOffset(original, previous, next)).NormalizePath();
            return;
        }

        bool isSpacedStream = !isOverlapping(original.GetEndPosition(), next.GetPosition());

        var secondNext = beatmap.HitObjects.GetNext(next);

        // We try to look ahead into the stream in an effort to determine the initial direction of a stream if we don't have a previous
        if (previous is null || !isChronologicallyClose(previous, original))
        {
            if (secondNext is not null)
                activeStreamDirection = getStreamDirection(next, original, secondNext);

            // Fallback to using the midpoint as best effort
            activeStreamDirection = getStreamDirection(original, null, next);
        }
        else
        {
            activeStreamDirection = getStreamDirection(original, previous, next);
        }

        int streamOffset = (int)activeStreamDirection;


        // Slides have special behavior in streams that make it so that the next note will share the same lane as the slide end
        if (converted is Slide slide)
            currentLane = slide.Lane + slide.SlideInfoList[0].SlidePath.EndLane;
        else
            currentLane += streamOffset;

        // If it is a spaced stream, we double the offset
        if (isSpacedStream)
            currentLane += activeStreamDirection == StreamDirection.Counterclockwise ? -1 : 1;

        currentLane = currentLane.NormalizePath();
    }

    private bool isOverlapping(Vector2 a, Vector2 b)
        => (b - a).LengthSquared <= Math.Pow(circleRadius * 2, 2);

    private bool isJump(HitObject original, HitObject next)
    {
        // If two notes are this distance apart, consider it a jump
        const float jump_distance_threshold_squared = 128 * 128;

        float distanceSqr = Vector2.DistanceSquared(original.GetEndPosition(), next.GetPosition());

        return distanceSqr >= jump_distance_threshold_squared;
    }

    private static int jumpLaneOffset(HitObject original, HitObject? previous, HitObject next)
    {
        Vector2 midpoint = midpointOf(original, previous, next);

        float angle1 = midpoint.GetDegreesFromPosition(original.GetEndPosition());
        float angle2 = midpoint.GetDegreesFromPosition(next.GetPosition());

        return getClosestLaneFor(angle2) - getClosestLaneFor(angle1);
    }

    private StreamDirection getStreamDirection(HitObject original, HitObject? previous, HitObject? next)
    {
        if (next is null)
            return activeStreamDirection;

        var midpoint = midpointOf(original, previous, next);

        float currAngle = midpoint.GetDegreesFromPosition(original.GetEndPosition());
        float nextAngle = midpoint.GetDegreesFromPosition(next.GetPosition());

        float angleDelta = SentakkiExtensions.GetDeltaAngle(currAngle, nextAngle);

        float absAngDelta = MathF.Abs(angleDelta);

        if (MathHelper.ApproximatelyEquivalent(absAngDelta, 0, 5))
            return activeStreamDirection;
        else if (MathHelper.ApproximatelyEquivalent(absAngDelta, 180, 5)) // Stream is collinear, equivalent to stack
            return StreamDirection.None;

        return angleDelta > 0 ? StreamDirection.Clockwise : StreamDirection.Counterclockwise;
    }

    private static Vector2 midpointOf(HitObject current, HitObject? previous, HitObject next)
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

        pointSum += next.GetPosition();
        pointCount++;

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

        return timeDelta <= beatLength;
    }
}
