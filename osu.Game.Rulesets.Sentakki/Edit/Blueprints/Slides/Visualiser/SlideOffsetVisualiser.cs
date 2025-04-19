using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides.Visualiser;

public partial class SlideOffsetVisualiser : CompositeDrawable, IHasTooltip
{
    private readonly Slide slide;
    private readonly SlideBodyInfo bodyInfo;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => InternalChildren.Any(c => c.ReceivePositionalInputAt(screenSpacePos));

    public LocalisableString TooltipText
    {
        get
        {
            double shootOffsetMS = bodyInfo.ShootDelay * editorBeatmap.ControlPointInfo.TimingPointAt(slide.StartTime).BeatLength;

            return $"Shoot offset: {shootOffsetMS:0.##}ms ({bodyInfo.ShootDelay:0.##} beats)";
        }
    }

    public SlideOffsetVisualiser(Slide slide, SlideBodyInfo bodyInfo)
    {
        this.slide = slide;
        this.bodyInfo = bodyInfo;
        Width = 50;

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
            new DragBox(slide, bodyInfo){
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

    private Color4 accentColour;
    private Color4 hoverAccentColour;

    [BackgroundDependencyLoader]
    private void load(OsuColour colours)
    {
        accentColour = Colour = colours.YellowDark;
        hoverAccentColour = accentColour.LightenHSL(0.5f);
    }

    [Resolved]
    private SentakkiSnapProvider snapProvider { get; set; } = null!;

    [Resolved]
    private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

    [Resolved]
    private EditorBeatmap editorBeatmap { get; set; } = null!;

    protected override void Update()
    {
        base.Update();

        double beatLength = editorBeatmap.ControlPointInfo.TimingPointAt(slide.StartTime).BeatLength;
        float dist = -snapProvider.GetDistanceRelativeToCurrentTime(slide.StartTime, SentakkiPlayfield.NOTESTARTDISTANCE);
        float distInner = -snapProvider.GetDistanceRelativeToCurrentTime(slide.StartTime + (bodyInfo.ShootDelay * beatLength), SentakkiPlayfield.NOTESTARTDISTANCE);
        Position = -SentakkiExtensions.GetPositionAlongLane(dist, 0);
        Height = Math.Abs(dist - distInner);
    }


    private bool isShootDelayValid(double shootDelay)
    {
        double beatLength = editorBeatmap.ControlPointInfo.TimingPointAt(slide.StartTime).BeatLength;

        double shootOffsetMS = shootDelay * beatLength;

        return shootOffsetMS >= 0 && shootOffsetMS < bodyInfo.Duration - 50;
    }


    protected override bool OnKeyDown(KeyDownEvent e)
    {
        switch (e.Key)
        {
            case Key.Plus:
            case Key.KeypadPlus:
            {
                float newShootDelay = bodyInfo.ShootDelay + 1f / beatSnapProvider.BeatDivisor;
                if (!isShootDelayValid(newShootDelay))
                    return true;

                editorBeatmap?.BeginChange();

                bodyInfo.ShootDelay = newShootDelay;
                editorBeatmap?.Update(slide);
                editorBeatmap?.EndChange();
                return true;
            }

            case Key.Minus:
            case Key.KeypadMinus:
            {
                float newShootDelay = bodyInfo.ShootDelay - (1f / beatSnapProvider.BeatDivisor);
                if (!isShootDelayValid(newShootDelay))
                    return true;

                editorBeatmap?.BeginChange();
                bodyInfo.ShootDelay = newShootDelay;
                editorBeatmap?.Update(slide);
                editorBeatmap?.EndChange();
                return true;
            }
        }
        return base.OnKeyDown(e);
    }

    protected override bool OnHover(HoverEvent e)
    {
        Colour = hoverAccentColour;
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        Colour = accentColour;
        base.OnHoverLost(e);
    }

    protected override bool OnClick(ClickEvent e) => true;
    protected override bool OnDragStart(DragStartEvent e) => true;

    private partial class DragBox : Box
    {
        public readonly Slide slide;
        public readonly SlideBodyInfo slideBodyInfo;

        public DragBox(Slide slide, SlideBodyInfo slideBodyInfo)
        {
            this.slide = slide;
            this.slideBodyInfo = slideBodyInfo;
        }

        protected override bool OnHover(HoverEvent e)
        {
            Colour = Color4.Red;

            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            Colour = Color4.White;
        }

        [Resolved]
        private SentakkiSnapProvider snapProvider { get; set; } = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override bool OnDragStart(DragStartEvent e) => true;
        protected override void OnDrag(DragEvent e)
        {
            var snapResult = snapProvider.GetSnapResult(e.ScreenSpaceMousePosition) ?? new SnapResult(e.ScreenSpaceMousePosition, null);

            snapResult.Time ??= snapProvider.GetDistanceBasedRawTime(e.ScreenSpaceMousePosition);

            double shootOffsetMS = snapResult.Time.Value - slide.StartTime;

            if (shootOffsetMS < 0 || shootOffsetMS >= slideBodyInfo.Duration - 50)
                return;

            double shootOffset = shootOffsetMS / editorBeatmap.ControlPointInfo.TimingPointAt(slide.StartTime).BeatLength;

            editorBeatmap?.BeginChange();
            slideBodyInfo.ShootDelay = Math.Max((float)shootOffset, 0);
            editorBeatmap?.Update(slide);
            editorBeatmap?.EndChange();
        }
    }
}
