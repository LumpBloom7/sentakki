using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class SentakkiSnapGrid : CompositeDrawable
{
    private DrawablePool<SnapGridLine> linePool = null!;

    private Bindable<double> animationDuration = new Bindable<double>(1000);

    private Container<SnapGridLine> linesContainer = null!;

    [Resolved]
    private EditorClock editorClock { get; set; } = null!;

    [Resolved]
    private EditorBeatmap editorBeatmap { get; set; } = null!;

    [Resolved]
    private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

    [Resolved]
    private OsuColour colours { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load(SentakkiRulesetConfigManager configManager)
    {
        Anchor = Origin = Anchor.Centre;
        AddRangeInternal(new Drawable[]{
            linePool = new DrawablePool<SnapGridLine>(10),
            linesContainer = new Container<SnapGridLine>
            {
                RelativeSizeAxes = Axes.Both,
            },
        });

        configManager.BindWith(SentakkiRulesetSettings.AnimationDuration, animationDuration);
    }

    public SnapResult GetSnapResult(Vector2 screenSpacePosition)
    {
        var localPosition = ToLocalSpace(screenSpacePosition);

        float length = localPosition.Length;

        var closestLine = linesContainer.MinBy(l => MathF.Abs(length - l.DrawWidth / 2));

        return new SentakkiSnapResult(screenSpacePosition, closestLine?.SnappingTime)
        {
            Lane = Vector2.Zero.GetDegreesFromPosition(localPosition).GetNoteLaneFromDegrees(),
            YPos = closestLine is not null ? closestLine.DrawWidth / 2 : SentakkiPlayfield.INTERSECTDISTANCE,
        };
    }

    private void recreateLines()
    {
        linesContainer.Clear(false);
        double time = editorClock.CurrentTime;

        double maximumVisibleTime = editorClock.CurrentTime + animationDuration.Value * 0.5f;
        double minimumVisibleTime = editorClock.CurrentTime - animationDuration.Value * 0.5f;

        for (int i = 0; i < editorBeatmap.ControlPointInfo.TimingPoints.Count; ++i)
        {
            var timingPoint = editorBeatmap.ControlPointInfo.TimingPoints[i];

            double nextTimingPointTime = i + 1 == editorBeatmap.ControlPointInfo.TimingPoints.Count ? editorBeatmap.BeatmapInfo.Length : editorBeatmap.ControlPointInfo.TimingPoints[i + 1].Time;

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

                SnapGridLine line;
                linesContainer.Add(line = linePool.Get());

                int divisor = BindableBeatDivisor.GetDivisorForBeatIndex(beatIndex, beatSnapProvider.BeatDivisor);

                float thickness = getWidthForDivisor(divisor);

                line.Size = new Vector2(circleRadius * 2);
                line.BorderThickness = thickness;
                line.Colour = BindableBeatDivisor.GetColourFor(divisor, colours);
                line.SnappingTime = beatTime;
            }
        }
    }

    private int getWidthForDivisor(int divisor) => divisor switch
    {
        1 or 2 => 5,
        3 or 4 => 4,
        6 or 8 => 3,
        _ => 2,
    };

    protected override void Update()
    {
        recreateLines();
        base.Update();
    }

    public float GetDistanceRelativeToCurrentTime(double time, float min = float.MinValue, float max = float.MaxValue)
    {
        double offsetRatio = (time - editorClock.CurrentTime) / (animationDuration.Value * 0.5f);

        float distance = (float)Interpolation.Lerp(SentakkiPlayfield.INTERSECTDISTANCE, SentakkiPlayfield.NOTESTARTDISTANCE, offsetRatio);

        return (float)Math.Clamp(distance, min, max);
    }

    private partial class SnapGridLine : PoolableDrawable
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
                BorderColour = Color4.White,
                BorderThickness = 2,
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
