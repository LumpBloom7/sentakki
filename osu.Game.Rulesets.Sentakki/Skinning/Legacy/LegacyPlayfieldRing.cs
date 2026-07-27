using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public partial class LegacyPlayfieldRing : CompositeDrawable
{
    public LegacyPlayfieldRing()
    {
        Size = new Vector2(600);
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        Alpha = 1;
    }

    [BackgroundDependencyLoader]
    private void load(ISkinSource skin)
    {
        AddInternal(new Sprite
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,

            RelativeSizeAxes = Axes.Both,
            FillMode = FillMode.Fit,
            Texture = skin.GetTexture("sentakki/playfield-ring")
        });
    }
}
