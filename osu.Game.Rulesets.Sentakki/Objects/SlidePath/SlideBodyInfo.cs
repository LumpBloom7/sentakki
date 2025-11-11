using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.SlidePath;

/// <summary>
/// This is the model of the slide body, containing essential information that defines the body.
/// </summary>
public class SlideBodyInfo
{
    #region Properties

    public bool Break;
    public bool Ex;

    // The amount of milliseconds before the star launches
    public double HoldDuration;

    // The total duration of the slide body.
    public double Duration;

    public double MovementDuration => Duration - HoldDuration;

    private readonly List<SlideSegment> segments = new List<SlideSegment>();

    [JsonIgnore]
    public int EndLane { get; private set; }

    public IReadOnlyList<SlideSegment> Segments
    {
        get => segments;
        set
        {
            if (segments.SequenceEqual(value))
                return;

            segments.Clear();
            segments.AddRange(value);

            EndLane = value.Sum(s => s.EndOffset);
            invalidatePath();
        }
    }

    #endregion

    #region Backing state

    private readonly Bindable<int> version = new Bindable<int>();

    [JsonIgnore]
    public IBindable<int> Version => version;

    private readonly List<SliderPath> segmentPaths = [];
    private readonly List<double> segmentStartProgress = [];

    public IReadOnlyList<SliderPath> SegmentPaths
    {
        get
        {
            validatePath();
            return segmentPaths;
        }
    }

    private double totalLength;

    public double SlideLength
    {
        get
        {
            validatePath();
            return totalLength;
        }
    }

    private readonly Cached pathCache = new Cached();

    private void invalidatePath()
    {
        pathCache.Invalidate();
        ++version.Value;
    }

    private void validatePath()
    {
        if (pathCache.IsValid)
            return;

        segmentPaths.Clear();
        segmentPaths.AddRange(SlidePaths.CreateSlidePath(segments));
        totalLength = segmentPaths.Sum(s => s.CalculatedDistance);

        double currentDistance = 0;

        foreach (var segmentPath in segmentPaths)
        {
            segmentStartProgress.Add(currentDistance / totalLength);
            currentDistance += segmentPath.CalculatedDistance;
        }

        pathCache.Validate();
    }

    #endregion

    #region Methods

    public Vector2 PositionAt(float progress, int offset = 0)
    {
        validatePath();

        progress = Math.Clamp(progress, 0, 1);

        double distance = progress * totalLength;

        int startLane = 0;

        for (int i = 0; i < segments.Count; ++i)
        {
            var segmentPath = segmentPaths[i];

            if (distance > segmentPath.CalculatedDistance)
            {
                distance -= segmentPath.CalculatedDistance;
                startLane += segments[i].EndOffset;
                continue;
            }

            double progressInSegment = distance / segmentPath.CalculatedDistance;

            if (segments[i].Shape is not PathShapes.Fan || i != segments.Count - 1) return segmentPath.PositionAt(progressInSegment);

            // Special case for slide fans

            var startPos = segmentPath.PositionAt(0);
            var endPos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, startLane + segments[i].EndOffset + offset);

            return Vector2.Lerp(startPos, endPos, (float)progressInSegment);
        }

        return Vector2.Zero;
    }

    public double SegmentStartProgressFor(Index index)
    {
        validatePath();
        return segmentStartProgress[index];
    }

    public double RecommendedMinimumDuration
    {
        get
        {
            validatePath();
            return totalLength / 5;
        }
    }

    public double RecommendedMaximumDuration
    {
        get
        {
            validatePath();
            return totalLength * 2;
        }
    }

    #endregion
}
