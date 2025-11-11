using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
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

    private SlideBodyInfo? path = null!;

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

            if (LoadState < LoadState.Ready)
                return;

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
            UpdateChevronVisibility();
        }
    }

    public void UpdateProgress(SlideChevron chevron)
    {
        chevron.Alpha = Progress >= chevron.DisappearThreshold ? 0 : 1;
    }

    public void UpdateChevronVisibility()
    {
        for (int i = 0; i < chevrons.Count; i++)
            UpdateProgress(chevrons[i]);
    }

    public SlideVisual()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        AutoSizeAxes = Axes.Both;
    }

    [Resolved]
    private DrawablePool<SlideChevron>? chevronPool { get; set; }

    [Resolved(canBeNull: true)]
    private DrawableHitObject? drawableHitObject { get; set; }

    private Container<SlideChevron> chevrons = null!;

    private readonly BindableBool snakingIn = new BindableBool(true);

    [BackgroundDependencyLoader]
    private void load(SentakkiRulesetConfigManager? sentakkiConfig)
    {
        sentakkiConfig?.BindWith(SentakkiRulesetSettings.SnakingSlideBody, snakingIn);

        AddRangeInternal([
            chevrons = []
        ]);

        updateVisuals();

        pathVersion.BindValueChanged(_ => updateVisuals());
    }

    protected override void Update()
    {
        base.Update();

        if (Time.Current < (drawableHitObject?.StartTimeBindable.Value ?? double.MinValue))
            return;

        if (drawableHitObject?.Result.HasResult ?? false)
            return;

        UpdateChevronVisibility();
    }

    private void updateVisuals()
    {
        chevrons.Clear(false);

        if (path is null)
            return;

        // Create regular slide chevrons if needed
        tryCreateRegularChevrons();

        // Create fan slide chevrons if needed
        tryCreateFanChevrons();
    }

    private const int chevrons_per_eith = 9;
    private const double ring_radius = 297;
    private const double chevrons_per_distance = chevrons_per_eith * 8 / (2 * Math.PI * ring_radius);
    private const double endpoint_distance = 30; // margin for each end

    private static int chevronsInContinuousPath(SliderPath path)
    {
        return (int)Math.Ceiling((path.Distance - 2 * endpoint_distance) * chevrons_per_distance);
    }

    private void tryCreateRegularChevrons()
    {
        Debug.Assert(chevronPool is not null);
        Debug.Assert(path is not null);

        double runningDistance = 0;

        int regularSegments = path.SegmentPaths.Count;

        if (regularSegments == 0)
            return;

        if (path.Segments[^1].Shape is PathShapes.Fan)
            regularSegments -= 1;

        for (int i = 0; i < regularSegments; ++i)
        {
            var segment = path.SegmentPaths[i];

            int chevronCount = chevronsInContinuousPath(segment);
            double totalDistance = segment.Distance;
            double safeDistance = totalDistance - endpoint_distance * 2;

            var previousPosition = segment.PositionAt(0);
            SlideChevron? lastChevron = null;

            for (int j = 0; j < chevronCount; j++)
            {
                double progress = (double)j / (chevronCount - 1); // from 0 to 1, both inclusive
                double distance = progress * safeDistance + endpoint_distance;
                progress = distance / totalDistance;
                var position = segment.PositionAt(progress);
                float angle = previousPosition.AngleTo(position);

                var chevron = chevronPool.Get();
                chevron.Position = position;

                chevron.DisappearThreshold = (runningDistance + distance) / path.SlideLength;
                chevron.Rotation = angle;
                chevron.Depth = chevrons.Count;

                chevron.Thickness = 13f;
                chevron.Height = 30;
                chevron.FanChevron = false;
                chevron.Width = 50;

                if (((DrawableSentakkiHitObject?)drawableHitObject)?.ExBindable.Value ?? false)
                {
                    chevron.ShadowRadius = 7.5f;
                    chevron.Glow = true;
                }
                else
                {
                    chevron.ShadowRadius = 15f;
                    chevron.Glow = false;
                }

                // If we find that the chevrons are a bit too bunched up, we remove the previous
                if (lastChevron is not null)
                {
                    float angleDelta = Math.Abs(MathExtensions.AngleDelta(lastChevron.Rotation, chevron.Rotation));

                    if (angleDelta >= 90)
                        chevrons.Remove(lastChevron, false);
                }

                chevrons.Add(chevron);
                lastChevron = chevron;

                previousPosition = position;
            }

            runningDistance += totalDistance;
        }
    }

    private void tryCreateFanChevrons()
    {
        Debug.Assert(chevronPool is not null);
        Debug.Assert(path is not null);

        if (path.Segments.Count == 0)
            return;

        if (path.Segments[^1].Shape is not PathShapes.Fan)
            return;

        var fanOrigin = path.SegmentPaths[^1].PositionAt(0);
        double fanStartProgress = path.SegmentStartProgressFor(^1);

        var delta = path.PositionAt(1) - fanOrigin;
        Vector2 lineStart = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0);
        Vector2 middleLineEnd = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 4);
        Vector2 middleLineDelta = middleLineEnd - lineStart;

        for (int i = 0; i < 11; ++i)
        {
            float progress = (i + 2f) / 12f;

            float scale = progress - 1f / 12f;
            var middlePosition = lineStart + middleLineDelta * progress;

            float t = 6.5f + 2.5f * scale;

            float chevWidth = MathF.Abs(lineStart.X - middlePosition.X) - t;

            (float sin, float cos) = MathF.SinCos((-135 + 90f) / 180f * MathF.PI);

            Vector2 secondPoint = new Vector2(sin, -cos) * chevWidth;
            Vector2 one = new Vector2(chevWidth, 0);

            var middle = (one + secondPoint) * 0.5f;
            float h = (middle - Vector2.Zero).Length + t * 3;

            float w = (secondPoint - one).Length;

            const float safe_space_ratio = 570 / 600f;

            float y = safe_space_ratio * scale;

            var chevron = chevronPool.Get();

            chevron.Position = fanOrigin + delta * y;
            chevron.Rotation = chevron.Position.AngleTo(path.PositionAt(1));

            chevron.DisappearThreshold = fanStartProgress + (i + 1) / 11f * (1 - fanStartProgress);
            chevron.Depth = chevrons.Count;

            chevron.Width = w;
            chevron.Height = h;
            chevron.Thickness = t * 2;
            chevron.FanChevron = true;

            if (((DrawableSentakkiHitObject?)drawableHitObject)?.ExBindable.Value ?? false)
            {
                chevron.ShadowRadius = 7.5f;
                chevron.Glow = true;
            }
            else
            {
                chevron.ShadowRadius = 15f;
                chevron.Glow = false;
            }

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

    public void PerformExitAnimation(double duration, double hitTime)
    {
        int i;

        // First rehide any chevrons that are part of completed segments
        // Required because the entry animation will unconditionally fade them back in, and Update() will not change anything post judgement
        for (i = chevrons.Count - 1; i >= 0; --i)
        {
            var chevron = chevrons[i];
            if (chevron.DisappearThreshold > Progress)
                break;

            chevron.FadeOut();
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
