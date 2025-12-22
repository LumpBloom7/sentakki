
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Snapping;

public partial class LaneNoteSnapGrid : VisibilityContainer
{
    [Resolved]
    private EditorClock editorClock { get; set; } = null!;

    [Resolved]
    private EditorBeatmap editorBeatmap { get; set; } = null!;

    [Resolved]
    private BindableBeatDivisor bindableBeatDivisor { get; set; } = null!;

    [Resolved]
    private OsuColour colours { get; set; } = null!;

    protected override void PopIn() => this.FadeIn(50);
    protected override void PopOut() => this.FadeOut(100);

    private Cached gridCache = new Cached();

    private Bindable<double> animationDuration = new Bindable<double>();

    private DrawablePool<DrawableGridLine> linePool = null!;

    private Container<DrawableGridLine> gridLines = null!;

    [BackgroundDependencyLoader]
    private void load(SentakkiHitObjectComposer composer)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;

        animationDuration.BindTo(composer.DrawableRuleset.AdjustedAnimDuration);
        animationDuration.BindValueChanged(_ => gridCache.Invalidate());

        bindableBeatDivisor.BindValueChanged(_ => gridCache.Invalidate());

        AddRangeInternal([
            gridLines = new Container<DrawableGridLine>
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            },
            linePool = new DrawablePool<DrawableGridLine>(20)
        ]);
    }

    protected override void Update()
    {
        base.Update();

        if (editorClock.ElapsedFrameTime != 0)
            gridCache.Invalidate();

        regenerateGrid();
    }

    private void regenerateGrid()
    {
        if (gridCache.IsValid)
            return;

        gridLines.Clear(false);

        double currentTime = editorClock.CurrentTime;
        double maximumTime = currentTime + animationDuration.Value / 2;

        var timingPoints = editorBeatmap.ControlPointInfo.TimingPoints;

        for (int i = 0; i < timingPoints.Count; ++i)
        {
            if (i < timingPoints.Count - 1 && timingPoints[i + 1].Time <= currentTime)
                continue;

            var timingPoint = timingPoints[i];

            if (timingPoint.Time > maximumTime)
                return;


            double beatLength = timingPoint.BeatLength / bindableBeatDivisor.Value;

            for (int beatIndex = 0; ; ++beatIndex)
            {
                double beatTime = timingPoint.Time + beatIndex * beatLength;

                if (beatTime > maximumTime)
                    break;

                if (beatTime <= currentTime)
                    continue;

                double scaleAmount = 1 - ((beatTime - currentTime) / (animationDuration.Value / 2));

                var line = linePool.Get();

                line.Size = new Vector2((float)Interpolation.Lerp(SentakkiPlayfield.NOTESTARTDISTANCE, SentakkiPlayfield.INTERSECTDISTANCE, scaleAmount)) * 2;

                int divisor = BindableBeatDivisor.GetDivisorForBeatIndex(beatIndex, bindableBeatDivisor.Value);
                line.Colour = BindableBeatDivisor.GetColourFor(divisor, colours);
                line.Thickness = BindableBeatDivisor.GetSize(divisor).Y * 2;

                gridLines.Add(line);
            }
        }
    }

    private partial class DrawableGridLine : PoolableDrawable
    {
        private CircularProgress circularProgress;

        private float thickness;

        public float Thickness
        {
            get => thickness;
            set
            {
                if (value == thickness)
                    return;

                thickness = value;
                circularProgress.InnerRadius = value / 300;
            }
        }

        public DrawableGridLine()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AddInternal(circularProgress = new CircularProgress
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Progress = 1,
            });
        }
    }
}