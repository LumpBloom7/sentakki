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
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides.Visualiser;

public partial class SlideOffsetVisualiser : VisibilityContainer, IHasTooltip
{
    private readonly Slide slide;
    private readonly SlideBodyInfo bodyInfo;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => InternalChildren.Any(c => c.ReceivePositionalInputAt(screenSpacePos));

    public LocalisableString TooltipText
    {
        get
        {
            double shootDelayBeats = bodyInfo.ShootDelay / editorBeatmap.ControlPointInfo.TimingPointAt(slide.StartTime).BeatLength;

            return $"Shoot offset: {bodyInfo.ShootDelay:0.##}ms ({shootDelayBeats:0.##} beats)";
        }
    }

    public SlideOffsetVisualiser(Slide slide, SlideBodyInfo bodyInfo)
    {
        this.slide = slide;
        this.bodyInfo = bodyInfo;
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
                EdgeSmoothness = new Vector2(1),
            },
            new Box
            {
                Width = 5,
                RelativeSizeAxes = Axes.Y,
                Anchor = Anchor.CentreRight,
                Origin = Anchor.Centre,
                EdgeSmoothness = new Vector2(1),
            },
            new DragHandle(slide, bodyInfo)
            {
                Height = 20,
                Width = 2 / 5f,
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.BottomRight,
                Origin = Anchor.Centre,
            },
        ];
    }

    private Color4 accentColour;
    private Color4 hoverAccentColour;

    [BackgroundDependencyLoader]
    private void load(OsuColour colours)
    {
        accentColour = Colour = colours.YellowDark;
        hoverAccentColour = accentColour.LightenHsl(0.5f);
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

        float dist = -snapProvider.GetDistanceRelativeToCurrentTime(slide.StartTime, SentakkiPlayfield.NOTESTARTDISTANCE);
        float distInner = -snapProvider.GetDistanceRelativeToCurrentTime(slide.StartTime + bodyInfo.ShootDelay, SentakkiPlayfield.NOTESTARTDISTANCE);
        Position = -SentakkiExtensions.GetPositionAlongLane(dist, 0);
        Height = Math.Abs(dist - distInner);
    }

    private bool isShootDelayValid(double shootDelay) => shootDelay >= 0 && shootDelay <= bodyInfo.Duration;

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        if (State.Value is Visibility.Hidden)
            return false;

        switch (e.Key)
        {
            case Key.D:
            {
                double newShootDelay = bodyInfo.ShootDelay + beatSnapProvider.GetBeatLengthAtTime(slide.StartTime);

                if (!isShootDelayValid(newShootDelay))
                    return true;

                editorBeatmap?.BeginChange();

                bodyInfo.ShootDelay = newShootDelay;
                editorBeatmap?.Update(slide);

                editorBeatmap?.EndChange();
                return true;
            }

            case Key.A:
            {
                double newShootDelay = bodyInfo.ShootDelay - beatSnapProvider.GetBeatLengthAtTime(slide.StartTime);

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

    private partial class DragHandle : FastCircle
    {
        private readonly Slide slide;
        private readonly SlideBodyInfo slideBodyInfo;

        public DragHandle(Slide slide, SlideBodyInfo slideBodyInfo)
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
            double snappedTime = snapProvider.GetDistanceBasedSnapTime(e.ScreenSpaceMousePosition);
            double shootDelay = snappedTime - slide.StartTime;

            shootDelay = Math.Clamp(shootDelay, 0, slideBodyInfo.Duration);

            if (shootDelay == slideBodyInfo.ShootDelay)
                return;

            editorBeatmap?.BeginChange();

            slideBodyInfo.ShootDelay = shootDelay;
            editorBeatmap?.Update(slide);

            editorBeatmap?.EndChange();
        }
    }

    protected override void PopIn()
    {
        Alpha = 1;
    }

    protected override void PopOut()
    {
        Alpha = 0;
    }
}
