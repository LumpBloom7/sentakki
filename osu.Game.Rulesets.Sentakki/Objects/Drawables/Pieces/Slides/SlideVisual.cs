using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;

public partial class SlideVisual : CompositeDrawable
{
    // This will be proxied, so a must.
    public override bool RemoveWhenNotAlive => false;

    // SSDQ.AABBFloat may return a Rectangle far larger than the actual bounding rect when rotated
    //  To avoid that, we manually compute a non rotated rect that fits all the chevrons.
    protected override Quad ComputeScreenSpaceDrawQuad()
    {
        if (chevrons.Count == 0)
            return base.ComputeScreenSpaceDrawQuad();

        var rect = chevrons[0].ScreenSpaceDrawQuad.AABBFloat;

        for (int i = 1; i < chevrons.Count; ++i)
            rect = RectangleF.Union(rect, chevrons[i].ScreenSpaceDrawQuad.AABBFloat);

        return Quad.FromRectangle(rect);
    }

    private SlideBodyInfo? path;

    private readonly IBindable<int> pathVersion = new Bindable<int>();

    public SlideBodyInfo? Path
    {
        get => path;
        set
        {
            pathVersion.UnbindAll();
            path = value;

            if (path is not null)
                pathVersion.BindTo(path.Version);

            updateVisuals();
        }
    }

    private double progress;

    public double Progress
    {
        get => progress;
        set
        {
            if (progress == value)
                return;

            progress = value;
            updateChevronVisibility();
        }
    }

    private void updateChevronVisibility()
    {
        foreach (var chevron in chevrons)
        {
            if (chevron.DisappearThreshold <= Progress)
                chevron.Hide();
            else
                chevron.Show();
        }
    }

    public SlideVisual()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        AddInternal(chevrons = new Container<SlideChevron>());
    }

    [Resolved]
    private DrawablePool<SlideChevron>? chevronPool { get; set; }

    [Resolved]
    private DrawableSentakkiHitObject? drawableHitObject { get; set; }

    private readonly Container<SlideChevron> chevrons;

    private readonly BindableBool snakingIn = new BindableBool(true);

    [BackgroundDependencyLoader]
    private void load(SentakkiRulesetConfigManager? sentakkiConfig)
    {
        sentakkiConfig?.BindWith(SentakkiRulesetSettings.SnakingSlideBody, snakingIn);
        pathVersion.BindValueChanged(_ => updateVisuals(), true);
    }

    private void updateVisuals()
    {
        chevrons.Clear(false);

        if (path is null)
            return;

        // There is a possibility that dependencies aren't injected yet
        // Defer the update of visuals until `load` is called
        if (chevronPool is null)
            return;

        // Create regular slide chevrons if needed
        createRegularChevrons();

        // Create fan slide chevrons if needed
        createFanChevrons();

        updateChevronVisibility();
    }

    private const int chevrons_per_eith = 9;
    private const double ring_radius = SentakkiPlayfield.INTERSECTDISTANCE;
    private const double chevrons_per_distance = chevrons_per_eith * 8 / (2 * Math.PI * ring_radius);
    private const double endpoint_distance = 30; // margin for each end

    private static int chevronsInContinuousPath(SliderPath path)
        => (int)Math.Ceiling((path.CalculatedDistance - 2 * endpoint_distance) * chevrons_per_distance);

    private void createRegularChevrons()
    {
        Debug.Assert(chevronPool is not null);
        Debug.Assert(path is not null);

        for (int i = 0; i < path.Segments.Count; ++i)
        {
            var segment = path.Segments[i];

            // We don't handle fan slides here
            if (i == path.Segments.Count - 1 && segment.Shape is PathShape.Fan)
                return;

            var segmentPath = path.SegmentPaths[i];
            double segmentStartProgress = path.SegmentStartProgressFor(i);

            // First we get the number of chevrons that is part of this segment
            int nChevrons = chevronsInContinuousPath(segmentPath);

            // Get the origin point of the segment, this is used to calculate the rotation of future chevrons
            Vector2 previousPosition = segmentPath.PositionAt(0);

            double margin = endpoint_distance / segmentPath.CalculatedDistance;
            double spacing = (1 - 2 * margin) / (nChevrons - 1);

            double segmentRatio = segmentPath.CalculatedDistance / path.SlideLength;

            SlideChevron? lastChevron = null;

            for (int j = 0; j < nChevrons; ++j)
            {
                double segmentProgress = margin + spacing * j;

                Vector2 position = segmentPath.PositionAt(segmentProgress);

                float rotation = previousPosition.AngleTo(position);
                previousPosition = position;

                // If there is a sudden change in angle like a right angle, the previous chevron may be not make any attempt to "turn the corner"
                // In such a case, we simply remove the previous chevron, and introduce a break in the slide path.
                if (lastChevron is not null)
                {
                    float angleDelta = Math.Abs(MathExtensions.AngleDelta(lastChevron.Rotation, rotation));

                    if (angleDelta >= 90)
                        chevrons.Remove(lastChevron, false);
                }

                // Prepare the chevron visual
                var chevron = lastChevron = chevronPool.Get();
                chevron.Position = position;
                chevron.Rotation = rotation;
                chevron.DisappearThreshold = segmentStartProgress + segmentProgress * segmentRatio;
                chevron.Size = new Vector2(50, 30);
                chevron.FanChevron = false;
                chevron.Glow = drawableHitObject?.ExBindable.Value ?? false;
                chevron.Depth = chevrons.Count; // Earlier chevrons should be drawn above later chevrons

                chevrons.Add(chevron);
            }
        }
    }

    private void createFanChevrons()
    {
        Debug.Assert(chevronPool is not null);
        Debug.Assert(path is not null);

        if (path.Segments.Count == 0 || path.Segments[^1].Shape is not PathShape.Fan)
            return;

        // All fans have 11 chevrons, this is exactly half the number of chevrons used by straight slides
        const int n_chevrons = 11;

        double fanStartProgress = path.SegmentStartProgressFor(^1);

        Vector2 fanOrigin = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, path.RelativeEndLane - 4);
        Vector2 middleLineEnd = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, path.RelativeEndLane);
        Vector2 leftLineEnd = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, path.RelativeEndLane - 1);
        Vector2 rightLineEnd = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, path.RelativeEndLane + 1);

        Vector2 middleVector = middleLineEnd - fanOrigin;
        Vector2 middleDirection = middleVector.Normalized();

        float margin = (float)endpoint_distance / middleVector.Length;
        float spacing = (1 - 2 * margin) / (n_chevrons - 1);

        // These are the endpoints of each line, with the margin taken into account
        var leftVecSafeEnd = fanOrigin + (leftLineEnd - fanOrigin) * (1 - margin);
        var rightVecSafeEnd = fanOrigin + (rightLineEnd - fanOrigin) * (1 - margin);
        var middleVecSafeEnd = fanOrigin + middleVector * (1 - margin);

        // The maximum width and height is derived using the largest possible chevron
        // We add (50,30) to the size because in order to take into account our desired minimum size.
        // The default thickness of 13 is subtracted from the maxW so that the edges of the fan perfectly point to lane 3 and 5
        float maxW = Vector2.Distance(leftVecSafeEnd, rightVecSafeEnd) + 50 - 13;
        var maxIntersect = Interpolation.ValueAt(0.5, leftVecSafeEnd, rightVecSafeEnd, 0, 1);
        float maxH = Vector2.Distance(maxIntersect, middleVecSafeEnd) + 30;

        double segmentRatio = 1 - fanStartProgress;

        // All fan chevrons are guaranteed to point the same way
        float rotation = fanOrigin.AngleTo(middleLineEnd);

        for (int i = 0; i < n_chevrons; ++i)
        {
            float segmentProgress = (float)i / n_chevrons;
            Vector2 position = fanOrigin + middleVector * (margin + i * spacing);

            float width = Interpolation.ValueAt(segmentProgress, 50, maxW, 0, 1);
            float height = Interpolation.ValueAt(segmentProgress, 30, maxH, 0, 1);

            // The position is based on the original bounding box of the chevron,
            // When we expand the bounding box to make larger chevrons, we apply a positional offset to account for the difference.
            float yOffset = (height - 30) / 2;

            // Prepare the chevron visual
            var chevron = chevronPool.Get();
            chevron.Position = position - yOffset * middleDirection;
            chevron.Rotation = rotation;
            chevron.Size = new Vector2(width, height);
            chevron.FanChevron = true;
            chevron.Glow = drawableHitObject?.ExBindable.Value ?? false;

            chevron.DisappearThreshold = fanStartProgress + (margin + segmentProgress) * segmentRatio;
            chevron.Depth = chevrons.Count; // Earlier chevrons should be drawn above later chevrons

            chevrons.Add(chevron);
        }
    }

    public void PerformEntryAnimation(double duration)
    {
        if (snakingIn.Value)
        {
            double fadeDuration = duration / chevrons.Count;
            double currentOffset = duration / 2;
            double offsetIncrement = (duration - currentOffset - fadeDuration) / (chevrons.Count - 1);

            for (int j = chevrons.Count - 1; j >= 0; j--)
            {
                var chevron = chevrons[j];
                chevron.FadeOut()
                       .Delay(currentOffset)
                       .FadeIn(fadeDuration);

                currentOffset += offsetIncrement;
            }
        }
        else
        {
            chevrons.FadeOut().Delay(duration / 2).FadeIn(duration / 2);
        }
    }

    public void PerformExitAnimation(double duration)
    {
        int i;

        for (i = chevrons.Count - 1; i >= 0; --i)
        {
            var chevron = chevrons[i];
            if (chevron.DisappearThreshold > Progress)
                break;
        }

        double fadeoutOffset = 0;
        double fadeoutDuration = duration / (i + 1);

        for (; i >= 0; --i)
        {
            var chevron = chevrons[i];

            chevron.FadeIn().Delay(fadeoutOffset).FadeOut(fadeoutDuration);
            fadeoutOffset += fadeoutDuration / 2;
        }
    }

    public void Free() => chevrons.Clear(false);
}
