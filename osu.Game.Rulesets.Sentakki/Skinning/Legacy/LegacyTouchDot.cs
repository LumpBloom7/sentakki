using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public partial class LegacyTouchDot : CompositeDrawable
{
    public LegacyTouchDot()
    {
        Size = new Vector2(20);
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
    }

    [BackgroundDependencyLoader]
    private void load(ISkinSource skin)
    {
        AddInternal(new Sprite
        {
            RelativeSizeAxes = Axes.Both,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Texture = skin.GetTexture("sentakki/glow/touch-dot"),
            Scale = new Vector2(1.5f),
            FillMode = FillMode.Fit,
            Colour = Color4.Black
        });

        AddInternal(new Sprite
        {
            RelativeSizeAxes = Axes.Both,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Texture = skin.GetTexture("sentakki/touch-dot"),
            FillMode = FillMode.Fit,
            Colour = Color4.White
        });
    }
}
