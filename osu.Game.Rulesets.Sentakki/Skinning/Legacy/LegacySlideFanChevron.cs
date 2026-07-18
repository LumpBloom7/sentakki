using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Sentakki.Skinning.Common;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public partial class LegacySlideFanChevron : CompositeDrawable
{
    private int index;
    public LegacySlideFanChevron(int index)
    {
        this.index = index;

        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    private IBindable<bool> exBindable = new Bindable<bool>();

    private Sprite glowSprite = null!;

    [BackgroundDependencyLoader]
    private void load(PoolableGameplayChevron? gameplayChevron, ISkinSource skin)
    {
        AddRangeInternal([
            glowSprite = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Scale = new Vector2(1.5f),

                Texture = skin.GetTexture($"sentakki/slide-fan-chevron-{index}-glow")
            },
            new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,

                Texture = skin.GetTexture($"sentakki/slide-fan-chevron-{index}")
            }
        ]);


        if (gameplayChevron is null)
            return;

        exBindable.BindTo(gameplayChevron.GlowBindable);
        exBindable.BindValueChanged(e => glowSprite.Colour = e.NewValue ? Color4.White : Color4.Black, true);
    }
}
