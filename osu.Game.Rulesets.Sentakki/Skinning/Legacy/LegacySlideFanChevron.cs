using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Sentakki.Skinning.Common;
using osu.Game.Skinning;

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

    [BackgroundDependencyLoader]
    private void load(PoolableGameplayChevron? gameplayChevron, ISkinSource skin)
    {
        AddInternal(new Sprite
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            FillMode = FillMode.Fit,

            Texture = skin.GetTexture($"sentakki/slide-fan-chevron-{index}")
        });
    }
}
