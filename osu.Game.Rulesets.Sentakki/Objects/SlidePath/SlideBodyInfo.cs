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

    /// <summary>
    /// The amount of milliseconds before the star launches as set by the user.
    /// </summary>
    /// <remarks>
    /// This may not be an appropriate wait duration, and can exceed the duration of the slide. Use <see cref="EffectiveWaitDuration"/> instead for gameplay purposes.
    /// </remarks>
    public double WaitDuration;

    // The total duration of the slide body.
    public double Duration;

    /// <summary>
    /// The amount of milliseconds before the star launches.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="WaitDuration"/>, this is corrected to fit within the slide's duration. This should be used for gameplay purposes.
    /// </remarks>
    public double EffectiveWaitDuration => Math.Clamp(WaitDuration, 0, Duration);

    public double EffectiveMovementDuration => Duration - EffectiveWaitDuration;

    private readonly List<SlideSegment> segments = [];

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
        segmentStartProgress.Clear();

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

    public Vector2 PositionAt(float progress, int starLaneOffset = 0)
    {
        validatePath();

        progress = Math.Clamp(progress, 0, 1);

        // First get the current segment based on `progress`
        int segmentIndex = 0;

        for (; segmentIndex < segments.Count; ++segmentIndex)
        {
            if (segmentStartProgress[segmentIndex] > progress)
                break;
        }

        // Get the start progress of the next segment, or 1 if the current segment is the last
        double segmentEnd = (segmentIndex == segments.Count) ? 1 : segmentStartProgress[segmentIndex];

        --segmentIndex;
        double segmentStart = segmentStartProgress[segmentIndex];

        // We transform the progress so that it is in relation to the current segment.
        progress = (float)((progress - segmentStart) / (segmentEnd - segmentStart));

        if (segments[segmentIndex].Shape is not PathShapes.Fan || segmentIndex != segments.Count - 1)
            return segmentPaths[segmentIndex].PositionAt(progress);

        // Special case for slide fans
        // When travelling along the slide path, the stars can be at three different positions, one of them will follow the main line, the others will go to adjacent lanes.
        var startPos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, EndLane - 4);
        var endPos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, EndLane + starLaneOffset);

        return Vector2.Lerp(startPos, endPos, progress);
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
