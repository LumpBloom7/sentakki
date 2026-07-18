using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;

namespace osu.Game.Rulesets.Sentakki.Skinning.Common;

public partial class PoolableGameplayChevron : PoolableDrawable
{
    public double DisappearThreshold { get; set; }
    public bool IsVisible => chevron.IsPresent && IsPresent;

    private Drawable chevron = null!;

    public Bindable<bool> GlowBindable { get; } = new Bindable<bool>();

    public bool Glow
    {
        get => GlowBindable.Value;
        set => GlowBindable.Value = value;
    }

    public PoolableGameplayChevron() { }

    public PoolableGameplayChevron(Drawable drawable)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        AddInternal(chevron = drawable);
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
