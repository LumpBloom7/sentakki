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
        set
        {
            chevron.Glow = value;
            chevron.ShadowRadius = value ? 15 : 7.5f;
        }
    }

    public SlideChevron()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        Size = new Vector2(50, 30);
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        AddInternal(chevron = new DrawableChevron
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both
        });
    }

    public override void Hide()
    {
        chevron.Alpha = 0;
    }

    public override void Show()
    {
        chevron.Alpha = 1;
    }
}
