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
        double time = editorClock.CurrentTime;

        double itime = time - animationDuration.Value * 0.5f;
        int lineIndex = 0;

        while (true)
        {
            double snappedTime = beatSnapProvider.SnapTime(itime);
            double divisorLength = beatSnapProvider.GetBeatLengthAtTime(snappedTime);

            var tcp = editorBeatmap.ControlPointInfo.TimingPointAt(snappedTime);
            int beatIndex = (int)Math.Round((snappedTime - tcp.Time) / divisorLength);

            itime = snappedTime + divisorLength;

            float circleRadius = GetDistanceRelativeToCurrentTime(snappedTime);

            if (circleRadius > SentakkiPlayfield.INTERSECTDISTANCE * 2 - SentakkiPlayfield.NOTESTARTDISTANCE)
                continue;
            else if (circleRadius < SentakkiPlayfield.NOTESTARTDISTANCE)
                break;

            SnapGridLine line;

            if (linesContainer.Count == lineIndex)
                linesContainer.Add(line = linePool.Get());
            else
                line = linesContainer[lineIndex];

            int divisor = BindableBeatDivisor.GetDivisorForBeatIndex(beatIndex, beatSnapProvider.BeatDivisor);

            line.Size = new Vector2(circleRadius * 2);
            line.Colour = BindableBeatDivisor.GetColourFor(divisor, colours);
            line.SnappingTime = snappedTime;
            ++lineIndex;
        }

        while (linesContainer.Count > lineIndex)
            linesContainer.Remove(linesContainer[lineIndex], false);
    }

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

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Origin = Anchor.Centre;

            AddInternal(new CircularContainer
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
