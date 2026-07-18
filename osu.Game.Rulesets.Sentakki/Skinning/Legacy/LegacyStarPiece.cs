using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public partial class LegacyStarPiece : CompositeDrawable
{
    public LegacyStarPiece()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    [BackgroundDependencyLoader]
    private void load(ISkinSource skin)
    {
        AddRangeInternal([
            new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                Texture = skin.GetTexture("sentakki/star")
            }
        ]);
    }
}
