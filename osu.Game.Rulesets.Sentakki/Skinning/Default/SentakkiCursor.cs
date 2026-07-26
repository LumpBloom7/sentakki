using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default;

public partial class SentakkiCursor : CompositeDrawable
{
    public SentakkiCursor()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        Size = new Vector2(50);
    }

    [BackgroundDependencyLoader]
    private void load(TextureStore textures)
    {
        AddInternal(new Sprite
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Texture = textures.Get("SentakkiIcon"),
            FillMode = FillMode.Fit
        });
    }
}
