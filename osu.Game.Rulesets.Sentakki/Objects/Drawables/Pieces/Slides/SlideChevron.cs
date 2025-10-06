using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;

public partial class SlideChevron : PoolableDrawable
{
    public double DisappearThreshold { get; set; }
    private DrawableChevron chevron = null!;

    public bool FanChevron
    {
        get => chevron.FanChevron;
        set => chevron.FanChevron = value;
    }

    public float Thickness
    {
        get => chevron.Thickness;
        set => chevron.Thickness = value;
    }

    public bool Glow
    {
        get => chevron.Glow;
        set => chevron.Glow = value;
    }

    public float ShadowRadius
    {
        get => chevron.ShadowRadius;
        set => chevron.ShadowRadius = value;
    }

    public SlideChevron()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        Size = new Vector2(80, 60);
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        AddInternal(chevron = new DrawableChevron
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            ShadowRadius = 7.5f,
            Thickness = 15f
        });
    }

    protected override void FreeAfterUse()
    {
        // This is used to ensure that the chevrons transforms are reverted to the initial state.
        // TODO: Investigate what clears the transforms without rewinding them...
        if (!RemoveCompletedTransforms)
            ApplyTransformsAt(double.MinValue, propagateChildren: true);
        ClearTransformsAfter(double.MinValue, propagateChildren: true);

        base.FreeAfterUse();
    }
}
