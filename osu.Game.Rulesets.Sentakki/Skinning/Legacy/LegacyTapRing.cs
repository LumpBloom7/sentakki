using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Skinning.Default;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public partial class LegacyTapRing : CompositeDrawable
{
    private Container accentContainer = null!;
    private Sprite glowSprite = null!;

    public LegacyTapRing()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        Size = new Vector2(TapRing.CIRCLE_RADIUS * 2);
    }

    private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

    private readonly IBindable<bool> exState = new Bindable<bool>();

    [BackgroundDependencyLoader]
    private void load(ISkinSource skin, DrawableHitObject? drawableObject)
    {
        Texture baseTexture = skin.GetTexture("sentakki/hitobjects/tap/base")!;
        Texture? glowTexture = skin.GetTexture("sentakki/hitobjects/tap/glow");

        Texture? overlayTexture = skin.GetTexture("sentakki/hitobjects/tap/overlay");

        AddRangeInternal([
            accentContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,

                Children = [
                    glowSprite = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Texture = glowTexture,
                        Blending = BlendingParameters.Additive,
                        Scale = new Vector2(1.5f),
                    },
                    new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        FillMode = FillMode.Fit,
                        RelativeSizeAxes = Axes.Both,
                        Texture = baseTexture,
                    }
                ]
            },
            new Sprite
            {
                RelativeSizeAxes  = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fit,
                Texture = overlayTexture,
            }
        ]);

        if (drawableObject is not DrawableSentakkiHitObject dsho)
            return;

        accentColour.BindTo(dsho.AccentColour);
        accentColour.BindValueChanged(colour => accentContainer.Colour = colour.NewValue, true);

        exState.BindTo(dsho.ExBindable);
        exState.BindValueChanged(ex => glowSprite.Colour = ex.NewValue ? Color4.White : Color4.Black, true);
    }
}
