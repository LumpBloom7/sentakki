using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

public partial class NewBeatmapConverter
{
    private const int stackDistanceSquared = 9;

    private double stackThreshold => timePreempt * beatmap.BeatmapInfo.StackLeniency;

    private RotationDirection? streamDirection;

    private Tap convertHitCircle(HitObject original, HitObject? previous, HitObject? next)
    {
        bool isStack = this.isStack(original, previous);
        bool isStream = this.isStream(original, previous);

        if (isStack || !isStream)
            streamDirection = null;
        else
            streamDirection = getStreamDirection(original, previous, next);

        bool isBreak = original.Samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH);

        int streamOffset = streamDirection == RotationDirection.Clockwise ? 1 : -1;

        currentLane = isStack
            ? currentLane
            : (isStream ? currentLane + streamOffset : patternGenerator.RNG.Next(8));

        Tap result = new Tap
        {
            Lane = currentLane.NormalizePath(),
            Samples = original.Samples,
            StartTime = original.StartTime,
            Break = isBreak
        };

        return result;
    }

    private bool isStack(HitObject original, HitObject? previous)
    {
        if (previous is null)
            return false;

        if (previous.GetEndTime() + stackThreshold < original.StartTime)
            return false;

        return Vector2Extensions.DistanceSquared(positionOf(original), positionOf(previous)) < stackDistanceSquared;
    }

    private bool isStream(HitObject original, HitObject? previous)
    {
        if (previous is null)
            return false;

        return Vector2Extensions.DistanceSquared(endPositionOf(original), endPositionOf(previous)) > Math.Pow(circleRadius * 2, 2);
    }

    private static Vector2 positionOf(HitObject hitObject)
        => ((IHasPosition)hitObject).Position;

    private static Vector2 endPositionOf(HitObject hitObject)
    {
        var startPos = ((IHasPosition)hitObject).Position;

        if (hitObject is IHasPathWithRepeats slider)
            return startPos + slider.Path.PositionAt(1);

        return startPos;
    }

    private RotationDirection getStreamDirection(HitObject original, HitObject? previous, HitObject? next)
    {
        if (previous is null || next is null)
            return streamDirection ?? RotationDirection.Clockwise;

        var midpoint = midPointOf(original, previous, next);

        float prevAngle = midpoint.GetDegreesFromPosition(endPositionOf(previous));
        float currAngle = midpoint.GetDegreesFromPosition(endPositionOf(original));

        float angleDelta = SentakkiExtensions.GetDeltaAngle(prevAngle, currAngle);

        if (angleDelta == 0)
            return streamDirection ?? RotationDirection.Clockwise;

        return Math.Abs((prevAngle + angleDelta) % 360 - currAngle) < 0.1 ? RotationDirection.Clockwise : RotationDirection.Counterclockwise;
    }

    private static Vector2 midPointOf(params HitObject?[] hitObjects)
    {
        float sumX = 0;
        float sumY = 0;

        int count = 0;

        foreach (var hitObject in hitObjects)
        {
            if (hitObject is null)
                continue;

            var pos = endPositionOf(hitObject);

            sumX += pos.X;
            sumY += pos.Y;
            count++;
        }

        return new Vector2(sumX, sumY) / count;
    }
}
