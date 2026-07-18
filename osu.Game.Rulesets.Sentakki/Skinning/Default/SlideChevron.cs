using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.Skinning.Common;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default;

public partial class SlideChevron : CompositeDrawable
{
    protected Chevron Chevron = null!;

    public SlideChevron()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    private IBindable<bool> exBindable = new Bindable<bool>();

    [BackgroundDependencyLoader]
    private void load(PoolableGameplayChevron? gameplayChevron)
    {
        AddInternal(Chevron = new Chevron
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both
        });

        if (gameplayChevron is null)
            return;

        exBindable.BindTo(gameplayChevron.GlowBindable);
        exBindable.BindValueChanged(e => setExState(e.NewValue), true);
    }

    private void setExState(bool ex)
    {
        Chevron.Glow = ex;
        Chevron.ShadowRadius = ex ? 15 : 7.5f;
    }
}
