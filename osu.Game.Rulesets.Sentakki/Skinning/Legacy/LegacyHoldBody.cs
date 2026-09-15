using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Skinning.Default;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public partial class LegacyHoldBody : CompositeDrawable
{
    public LegacyHoldBody()
    {
        RelativeSizeAxes = Axes.Both;
    }

    private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

    [BackgroundDependencyLoader]
    private void load(ISkinSource skin, DrawableHitObject? drawableHitObject)
    {
        var bodyTexture = skin.GetTexture("sentakki/hitobjects/hold/body", WrapMode.ClampToEdge, WrapMode.ClampToEdge);
        var headTexture = skin.GetTexture("sentakki/hitobjects/hold/head", WrapMode.ClampToEdge, WrapMode.ClampToEdge);
        var tailTexture = skin.GetTexture("sentakki/hitobjects/hold/tail", WrapMode.ClampToEdge, WrapMode.ClampToEdge);

        InternalChild = new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            RowDimensions = [
                new Dimension(GridSizeMode.Absolute, TapRing.CIRCLE_RADIUS),
                new Dimension(GridSizeMode.Distributed),
                new Dimension(GridSizeMode.Absolute, TapRing.CIRCLE_RADIUS)
            ],

            Content = new Drawable[][]
            {
                [
                    new Sprite
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Height = 2,

                        Texture = headTexture,
                    },
                ],

                [
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Texture = bodyTexture,
                        }
                    }

                ],

                [
                    new Sprite
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Height = 2,

                        Texture = tailTexture,
                    }
                ]
            }
        };

        if (drawableHitObject is null)
            return;

        accentColour.BindTo(drawableHitObject.AccentColour);
        accentColour.BindValueChanged(c => Colour = c.NewValue);
    }
}
