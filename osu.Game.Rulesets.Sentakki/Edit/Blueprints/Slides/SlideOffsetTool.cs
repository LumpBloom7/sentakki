using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;

public partial class SlideOffsetTool : VisibilityContainer, IHasTooltip
{
    private readonly Slide slide;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => InternalChildren.Any(c => c.ReceivePositionalInputAt(screenSpacePos));

    public Bindable<double> ShootDelayBindable { get; set; } = new Bindable<double>();
    public Bindable<bool> ManualAdjustmentsMade { get; } = new Bindable<bool>();

    [Resolved]
    private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

    [Resolved]
    private SentakkiSnapProvider snapProvider { get; set; } = null!;

    public LocalisableString TooltipText
    {
        get
        {
            double shootDelayBeats = ShootDelayBindable.Value / (beatSnapProvider.GetBeatLengthAtTime(slide.StartTime) * beatSnapProvider.BeatDivisor);

            return $"Shoot offset: {ShootDelayBindable.Value:0.##}ms ({shootDelayBeats:0.##} beats)";
        }
    }

    public SlideOffsetTool(Slide slide)
    {
        this.slide = slide;
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

    private Color4 accentColour;
    private Color4 hoverAccentColour;

    [BackgroundDependencyLoader]
    private void load(OsuColour colours)
    {
        accentColour = Colour = colours.YellowDark;
        hoverAccentColour = accentColour.LightenHsl(0.5f);
    }

    protected override void Update()
    {
        base.Update();

        float dist = -snapProvider.GetDistanceRelativeToCurrentTime(slide.StartTime, SentakkiPlayfield.NOTESTARTDISTANCE);
        float distInner = -snapProvider.GetDistanceRelativeToCurrentTime(slide.StartTime + ShootDelayBindable.Value, SentakkiPlayfield.NOTESTARTDISTANCE);
        Y = dist;
        Height = Math.Abs(dist - distInner);
    }

    protected override void PopIn()
    {
        Alpha = 1;
    }

    protected override void PopOut()
    {
        Alpha = 0;
    }

    protected override bool OnHover(HoverEvent e)
    {
        Colour = hoverAccentColour;

        return false;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        Colour = accentColour;
    }

    protected override bool OnClick(ClickEvent e) => true;
    protected override bool OnDragStart(DragStartEvent e) => true;

    private partial class DragHandle : FastCircle
    {
        private readonly SlideOffsetTool slideOffsetTool;

        public DragHandle(SlideOffsetTool offsetTool)
        {
            slideOffsetTool = offsetTool;
        }

        [Resolved]
        private SentakkiSnapProvider snapProvider { get; set; } = null!;

        protected override bool OnHover(HoverEvent e)
        {
            Colour = Color4.Red;

            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            Colour = Color4.White;
        }

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override bool OnDragStart(DragStartEvent e) => true;

        protected override void OnDrag(DragEvent e)
        {
            double snappedTime = snapProvider.GetDistanceBasedSnapTime(e.ScreenSpaceMousePosition);
            double shootDelay = snappedTime - slideOffsetTool.slide.StartTime;

            slideOffsetTool.ShootDelayBindable.Value = shootDelay;
            slideOffsetTool.ManualAdjustmentsMade.Value = true;
        }
    }
}
