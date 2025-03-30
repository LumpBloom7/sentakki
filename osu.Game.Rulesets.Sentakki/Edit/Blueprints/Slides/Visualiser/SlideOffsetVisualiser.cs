using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides.Visualiser;

public partial class SlideOffsetVisualiser : CompositeDrawable
{
    private readonly Slide slide;
    private readonly SlideBodyInfo bodyInfo;
    public SlideOffsetVisualiser(Slide slide, SlideBodyInfo bodyInfo)
    {
        this.slide = slide;
        this.bodyInfo = bodyInfo;
        InternalChildren = [
            new Box{
                Height = 3,
                Y = 1.5f,
                Width = 0.5f,
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.TopRight,
                Origin = Anchor.Centre,
                EdgeSmoothness = new Vector2(1),
            },
            new Box{
                Width = 3,
                RelativeSizeAxes = Axes.Y,
                Anchor = Anchor.CentreRight,
                Origin = Anchor.Centre,
                EdgeSmoothness = new Vector2(1),
            },
            new Box{
                Height = 3,
                Y = -1.5f,
                Width = 0.5f,
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.BottomRight,
                Origin = Anchor.Centre,
                EdgeSmoothness = new Vector2(1),
            },
        ];
    }

    [Resolved]
    private SentakkiSnapProvider snapProvider { get; set; } = null!;


    [Resolved]
    private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

    [Resolved]
    private EditorBeatmap editorBeatmap { get; set; } = null!;


    protected override void Update()
    {
        double bpm = editorBeatmap.ControlPointInfo.TimingPointAt(slide.StartTime).BeatLength;
        float dist = -snapProvider.GetDistanceRelativeToCurrentTime(slide.StartTime, SentakkiPlayfield.NOTESTARTDISTANCE);
        float distInner = -snapProvider.GetDistanceRelativeToCurrentTime(slide.StartTime + bodyInfo.ShootDelay * bpm, SentakkiPlayfield.NOTESTARTDISTANCE);
        Position = -SentakkiExtensions.GetPositionAlongLane(dist, 0);
        Height = Math.Abs(dist - distInner);
        base.Update();
    }

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        switch (e.Key)
        {
            case Key.Plus:
            case Key.KeypadPlus:
                editorBeatmap?.BeginChange();
                bodyInfo.ShootDelay += 1f / beatSnapProvider.BeatDivisor;
                editorBeatmap?.Update(slide);
                editorBeatmap?.EndChange();
                return true;

            case Key.Minus:
            case Key.KeypadMinus:
                editorBeatmap?.BeginChange();
                bodyInfo.ShootDelay = Math.Max(bodyInfo.ShootDelay - 1f / beatSnapProvider.BeatDivisor, 0);
                editorBeatmap?.Update(slide);
                editorBeatmap?.EndChange();
                return true;

        }
        return base.OnKeyDown(e);
    }
}
