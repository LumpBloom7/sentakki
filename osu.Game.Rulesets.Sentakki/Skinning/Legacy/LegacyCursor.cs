using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public partial class LegacyCursor : CompositeDrawable
{
    public LegacyCursor()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
    }

    [BackgroundDependencyLoader]
    private void load(ISkinSource skin)
    {
        AddInternal(new Sprite
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Texture = skin.GetTexture("cursor"),
        });
    }
}
