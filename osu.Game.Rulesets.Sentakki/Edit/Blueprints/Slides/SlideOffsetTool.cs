using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;

public partial class SlideOffsetTool : CompositeDrawable
{
    private readonly Slide slide;
    private readonly SlideBodyInfo slideBodyInfo;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => InternalChildren.Any(c => c.ReceivePositionalInputAt(screenSpacePos));

    [Resolved]
    private EditorBeatmap editorBeatmap { get; set; } = null!;

    [Resolved]
    private EditorClock editorClock { get; set; } = null!;

    public SlideOffsetTool(Slide slide, SlideBodyInfo slideBodyInfo)
    {
        this.slide = slide;
        this.slideBodyInfo = slideBodyInfo;

        Width = 50;

        InternalChildren =
        [
            new Box
            {
                Height = 5,
                Width = 0.5f,
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.TopRight,
                Origin = Anchor.Centre,
                EdgeSmoothness = Vector2.One,
            },
            new Box
            {
                Width = 5,
                RelativeSizeAxes = Axes.Y,
                Anchor = Anchor.CentreRight,
                Origin = Anchor.Centre,
                EdgeSmoothness = Vector2.One,
            },
            new DragHandle(this)
            {
                Height = 20,
                Width = 2 / 5f,
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.BottomRight,
                Origin = Anchor.Centre,
            },
        ];
    }
    private readonly Bindable<double> animationSpeed = new Bindable<double>(5);

    [BackgroundDependencyLoader]
    private void load(SentakkiBlueprintContainer blueprintContainer)
    {
        animationSpeed.BindTo(blueprintContainer.Composer.DrawableRuleset.AdjustedAnimDuration);
    }

    protected override void Update()
    {
        base.Update();

        Y = -Interpolation.ValueAt(
            slide.StartTime,
            SentakkiPlayfield.INTERSECTDISTANCE,
            SentakkiPlayfield.NOTESTARTDISTANCE,
            editorClock.CurrentTime,
            editorClock.CurrentTime + animationSpeed.Value / 2
        );

        float Y2 = -Interpolation.ValueAt(
            slide.StartTime + slideBodyInfo.EffectiveWaitDuration,
            SentakkiPlayfield.INTERSECTDISTANCE,
            SentakkiPlayfield.NOTESTARTDISTANCE,
            editorClock.CurrentTime,
            editorClock.CurrentTime + animationSpeed.Value / 2
        );

        Y2 = Math.Min(Y2, SentakkiPlayfield.NOTESTARTDISTANCE);

        Height = Math.Abs(Y - Y2);
    }

    protected override bool OnClick(ClickEvent e) => true;
    protected override bool OnDragStart(DragStartEvent e) => true;

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        switch (e.Key)
        {
            case Key.Plus:
            case Key.KeypadPlus:
            {
                double beatLength = editorBeatmap.GetBeatLengthAtTime(editorClock.CurrentTime);
                slideBodyInfo.WaitDuration = slideBodyInfo.EffectiveWaitDuration + beatLength;

                editorBeatmap.BeginChange();
                editorBeatmap.Update(slide);
                editorBeatmap.EndChange();

                return true;
            }

            case Key.Minus:
            case Key.KeypadMinus:
            {
                double beatLength = editorBeatmap.GetBeatLengthAtTime(editorClock.CurrentTime);
                slideBodyInfo.WaitDuration = slideBodyInfo.EffectiveWaitDuration - beatLength;

                editorBeatmap.BeginChange();
                editorBeatmap.Update(slide);
                editorBeatmap.EndChange();

                return true;
            }
        }

        return base.OnKeyDown(e);
    }

    private partial class DragHandle : DotPiece
    {
        private readonly SlideOffsetTool slideOffsetTool;

        public DragHandle(SlideOffsetTool offsetTool)
        {
            slideOffsetTool = offsetTool;
        }

        [Resolved]
        private LaneNoteSnapGrid snapProvider { get; set; } = null!;

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        protected override bool OnHover(HoverEvent e)
        {
            Colour = Color4.Red;
            this.ScaleTo(1.3f, 50);

            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            Colour = Color4.White;
            this.ScaleTo(1f, 100);
        }

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override bool OnDragStart(DragStartEvent e) => true;

        protected override void OnDrag(DragEvent e)
        {
            var localPosition = snapProvider.ToLocalSpace(e.ScreenSpaceMousePosition) - snapProvider.OriginPosition;
            (double snappedTime, _) = snapProvider.GetSnappedTimeAndPosition(editorClock.CurrentTime, localPosition);
            double shootDelay = snappedTime - slideOffsetTool.slide.StartTime;

            slideOffsetTool.slideBodyInfo.WaitDuration = shootDelay;

            editorBeatmap.Update(slideOffsetTool.slide);
        }
    }
}
