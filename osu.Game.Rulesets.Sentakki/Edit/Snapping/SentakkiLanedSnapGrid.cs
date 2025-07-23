using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Snapping;

public partial class SentakkiLanedSnapGrid : CompositeDrawable
{
    private Bindable<TernaryState> enabled = new Bindable<TernaryState>(TernaryState.True);

    private DrawablePool<BeatSnapGridLine> linePool = null!;

    private Bindable<double> animationDuration = new Bindable<double>(1000);

    private Container<BeatSnapGridLine> linesContainer = null!;

    [Resolved]
    private SentakkiHitObjectComposer composer { get; set; } = null!;

    [Resolved]
    private EditorClock editorClock { get; set; } = null!;

    [Resolved]
    private EditorBeatmap editorBeatmap { get; set; } = null!;

    [Resolved]
    private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

    [Resolved]
    private Bindable<WorkingBeatmap> working { get; set; } = null!;

    [Resolved]
    private OsuColour colours { get; set; } = null!;

    public DrawableTernaryButton CreateTernaryButton() => new DrawableTernaryButton
    {
        Current = enabled,
        Description = "Lane beat snap",
        CreateIcon = () => new SpriteIcon { Icon = FontAwesome.Solid.Ruler }
    };

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Origin = Anchor.Centre;
        AddRangeInternal(new Drawable[]
        {
            linePool = new DrawablePool<BeatSnapGridLine>(10),
            linesContainer = new Container<BeatSnapGridLine>
            {
                RelativeSizeAxes = Axes.Both,
            },
        });

        animationDuration.BindTo(composer.DrawableRuleset.AdjustedAnimDuration);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        enabled.BindValueChanged(toggleVisibility);
    }

    private void toggleVisibility(ValueChangedEvent<TernaryState> v)
    {
        if (v.NewValue is TernaryState.True)
        {
            linesContainer.Show();
            return;
        }

        linesContainer.Hide();
    }

    public SnapResult GetSnapResult(Vector2 screenSpacePosition)
    {
        var localPosition = ToLocalSpace(screenSpacePosition);
        int lane = Vector2.Zero.GetDegreesFromPosition(localPosition).GetNoteLaneFromDegrees();

        if (enabled.Value is not TernaryState.True)
            return new SentakkiLanedSnapResult(screenSpacePosition, lane, null) { YPos = SentakkiPlayfield.INTERSECTDISTANCE };

        float length = localPosition.Length;

        var closestLine = linesContainer.MinBy(l => MathF.Abs(length - (l.DrawWidth / 2)));

        return new SentakkiLanedSnapResult(screenSpacePosition, lane, closestLine?.SnappingTime)
        {
            YPos = closestLine is not null ? closestLine.DrawWidth / 2 : SentakkiPlayfield.INTERSECTDISTANCE,
        };
    }

    public double SnappedDistanceTime(Vector2 screenSpacePosition)
    {
        const float min = SentakkiPlayfield.NOTESTARTDISTANCE;
        const float max = SentakkiPlayfield.INTERSECTDISTANCE - SentakkiPlayfield.NOTESTARTDISTANCE;

        var localPosition = ToLocalSpace(screenSpacePosition);

        // the distance to intersect distance, as a ratio of the max travel distance.
        float distanceRatioToIntersect = 1 - Math.Clamp((localPosition.Length - min) / max, 0, 1);
        double timeRange = animationDuration.Value * 0.5f;

        double unsnappedTime = editorClock.CurrentTimeAccurate + distanceRatioToIntersect * timeRange;

        return beatSnapProvider.SnapTime(unsnappedTime);
    }

    private void recreateLines()
    {
        linesContainer.Clear(false);
        double time = editorClock.CurrentTime;
        double animationDuration = this.animationDuration.Value;

        double maximumVisibleTime = editorClock.CurrentTime + (animationDuration * 0.5f);
        double minimumVisibleTime = editorClock.CurrentTime - (animationDuration * 0.5f);

        for (int i = 0; i < editorBeatmap.ControlPointInfo.TimingPoints.Count; ++i)
        {
            var timingPoint = editorBeatmap.ControlPointInfo.TimingPoints[i];

            double nextTimingPointTime = i + 1 == editorBeatmap.ControlPointInfo.TimingPoints.Count ? working.Value.Track.Length : editorBeatmap.ControlPointInfo.TimingPoints[i + 1].Time;

            // This timing point isn't visible from the current time (too late), subsequent timing points are later, so no need to consider them as well.
            if (timingPoint.Time > maximumVisibleTime)
                return;

            // The current timing point isn't visible from the current time as it is too early
            if (nextTimingPointTime <= minimumVisibleTime)
                continue;

            double beatLength = timingPoint.BeatLength / beatSnapProvider.BeatDivisor;

            for (int beatIndex = 0; timingPoint.Time + (beatIndex * beatLength) < nextTimingPointTime; ++beatIndex)
            {
                double beatTime = timingPoint.Time + (beatIndex * beatLength);

                if (beatTime > maximumVisibleTime)
                    return;

                if (beatTime < minimumVisibleTime)
                    continue;

                float circleRadius = GetDistanceRelativeToCurrentTime(beatTime);

                BeatSnapGridLine line = linePool.Get();

                int divisor = BindableBeatDivisor.GetDivisorForBeatIndex(beatIndex, beatSnapProvider.BeatDivisor);

                float thickness = getWidthForDivisor(divisor);

                line.Size = new Vector2(circleRadius * 2);
                line.BorderThickness = thickness * 2;
                line.Colour = BindableBeatDivisor.GetColourFor(divisor, colours);
                line.Alpha = 0.1f + ((float)Math.Pow(1 - (Math.Abs(beatTime - time) / (animationDuration * 0.5f)), 1.5f) * 0.9f);
                line.SnappingTime = beatTime;

                linesContainer.Add(line);
            }
        }
    }

    private float getWidthForDivisor(int divisor) => divisor switch
    {
        1 or 2 => 1,
        3 or 4 => 0.8f,
        6 or 8 => .7f,
        _ => 0.6f,
    };

    private double lastEditorTime = double.MinValue;
    private int lastBeatDivisor = 0;
    private double lastAnimationDuration = 1000;

    protected override void Update()
    {
        if (editorClock.CurrentTime != lastEditorTime || lastBeatDivisor != beatSnapProvider.BeatDivisor || lastAnimationDuration != animationDuration.Value)
        {
            recreateLines();
            lastEditorTime = editorClock.CurrentTime;
            lastBeatDivisor = beatSnapProvider.BeatDivisor;
            lastAnimationDuration = animationDuration.Value;
        }

        base.Update();
    }

    public float GetDistanceRelativeToCurrentTime(double time, float min = float.MinValue, float max = float.MaxValue)
    {
        double animationDuration = this.animationDuration.Value;

        double offsetRatio = (time - editorClock.CurrentTime) / (animationDuration * 0.5f);

        float distance = (float)Interpolation.Lerp(SentakkiPlayfield.INTERSECTDISTANCE, SentakkiPlayfield.NOTESTARTDISTANCE, offsetRatio);

        return Math.Clamp(distance, min, max);
    }

    private partial class BeatSnapGridLine : PoolableDrawable
    {
        public override bool RemoveCompletedTransforms => false;

        public double SnappingTime { get; set; }

        public new float BorderThickness
        {
            get => ring.BorderThickness;
            set => ring.BorderThickness = value;
        }

        private CircularContainer ring = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Origin = Anchor.Centre;

            AddInternal(ring = new CircularContainer
            {
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                BorderThickness = 1,
                BorderColour = Color4.White,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    AlwaysPresent = true
                }
            });
        }
    }
}
