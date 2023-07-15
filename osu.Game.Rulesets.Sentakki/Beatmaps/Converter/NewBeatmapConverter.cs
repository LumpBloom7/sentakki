using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

public partial class NewBeatmapConverter
{
    private static readonly Vector2 standard_playfield_size = new Vector2(512, 384);
    private static readonly Vector2 standard_playfield_center = standard_playfield_size / 2;

    private ConversionExperiments conversionExperiments;

    private int currentLane;

    private readonly double timePreempt;
    private readonly float circleRadius;

    private readonly IBeatmap beatmap;

    private readonly SentakkiPatternGenerator patternGenerator;

    private RotationDirection? activeStreamDirection;

    private const int stackDistanceSquared = 9;

    private double stackThreshold => timePreempt * beatmap.BeatmapInfo.StackLeniency;

    public NewBeatmapConverter(IBeatmap beatmap, ConversionExperiments experiments)
    {
        this.beatmap = beatmap;

        this.conversionExperiments = experiments;

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

            if (i > 0)
                previous = beatmap.HitObjects[i - 1];
            if (i < beatmap.HitObjects.Count - 1)
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
        bool isJump = this.isJump(original, next);
        bool isStack = this.isStack(original, next);
        bool isStream = this.isStream(original, next);

        if (isJump || isStack)
        {
            activeStreamDirection = null;

            if (isJump)
            {
                currentLane += jumpLaneOffset(original, next);
            }
            else if (isStack)
            {
                // No assign
            }
        }
        else if (isStream)
        {
            // Slides have special behavior
            if (result is Slide slide)
            {
                if (!isStack && isStream)
                    currentLane = slide.Lane + slide.SlideInfoList[0].SlidePath.EndLane;
            }
            else
            {
                activeStreamDirection = getStreamDirection(original, previous, next);
                int streamOffset = activeStreamDirection == RotationDirection.Clockwise ? 1 : -1;
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
        if (next is null)
            return false;

        if (original.GetEndTime() + stackThreshold < next.StartTime)
            return false;

        return Vector2Extensions.DistanceSquared(original.GetPosition(), next.GetPosition()) < stackDistanceSquared;
    }

    private bool isStream(HitObject original, HitObject? next)
    {
        if (next is null)
            return false;

        return Vector2Extensions.DistanceSquared(original.GetEndPosition(), next.GetPosition()) > Math.Pow(circleRadius * 2, 2);
    }

    private bool isJump(HitObject original, HitObject? next)
    {
        const float jump_distance_threshold_squared = 128 * 128;

        if (next is null)
            return false;

        float distanceSqr = Vector2.DistanceSquared(original.GetEndPosition(), next.GetPosition());

        return distanceSqr >= jump_distance_threshold_squared;
    }

    private int jumpLaneOffset(HitObject original, HitObject? next)
    {
        if (next is null)
            return 0;

        var midPoint = (original.GetEndPosition() + next.GetPosition()) * 0.5f;

        float angle1 = midPoint.GetDegreesFromPosition(original.GetEndPosition());
        float angle2 = midPoint.GetDegreesFromPosition(next.GetPosition());

        return getClosestLaneFor(angle2) - getClosestLaneFor(angle1);
    }

    private RotationDirection getStreamDirection(HitObject original, HitObject? previous, HitObject? next)
    {
        if (previous is null || next is null)
            return activeStreamDirection ?? RotationDirection.Clockwise;

        var midpoint = midPointOf(original, previous, next);

        float currAngle = midpoint.GetDegreesFromPosition(original.GetEndPosition());
        float nextAngle = midpoint.GetDegreesFromPosition(next.GetPosition());

        float angleDelta = SentakkiExtensions.GetDeltaAngle(currAngle, nextAngle);

        if (angleDelta == 0)
            return activeStreamDirection ?? RotationDirection.Clockwise;

        return Math.Abs((currAngle + angleDelta) % 360 - nextAngle) < 0.1 ? RotationDirection.Clockwise : RotationDirection.Counterclockwise;
    }

    private static Vector2 midPointOf(HitObject current, HitObject previous, HitObject next)
        => (current.GetPosition() + previous.GetEndPosition() + next.GetPosition()) / 3;

    private static int getClosestLaneFor(float angle)
    {
        float minDelta = float.MaxValue;
        int closestLane = -1;

        for (int i = 0; i < 8; ++i)
        {
            float delta = Math.Abs(i.GetRotationForLane() - angle);

            if (!(delta < minDelta)) continue;

            closestLane = i;
            minDelta = delta;
        }

        return closestLane;
    }
}
