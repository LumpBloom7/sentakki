using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public partial class LegacyTapPiece : CompositeDrawable
{
    private Sprite glowSprite = null!;

    public LegacyTapPiece()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

    private readonly IBindable<bool> exBindable = new Bindable<bool>();

    [BackgroundDependencyLoader]
    private void load(DrawableHitObject? drawableObject, ISkinSource skin)
    {
        InternalChildren = [
            glowSprite = new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(1.5f),
                Texture = skin.GetTexture("sentakki/glow/tap")
            },
            new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Texture = skin.GetTexture("sentakki/tap")
            }
        ];

        if (drawableObject is not DrawableSentakkiHitObject dsho)
            return;

        accentColour.BindTo(dsho.AccentColour);
        accentColour.BindValueChanged(colour => Colour = colour.NewValue, true);

        exBindable.BindTo(dsho.ExBindable);
        exBindable.BindValueChanged(e => glowSprite.Colour = e.NewValue ? Color4.White : Color4.Black, true);
    }
}
