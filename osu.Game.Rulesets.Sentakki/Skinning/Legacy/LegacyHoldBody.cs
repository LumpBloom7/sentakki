using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public partial class LegacyHoldBody : CompositeDrawable
{
    //public override Quad ScreenSpaceDrawQuad => noteVisuals.ScreenSpaceDrawQuad;

    private Container noteVisuals;

    private Sprite headGlow = null!;
    private Sprite midGlow = null!;
    private Sprite tailGlow = null!;

    public LegacyHoldBody()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.Both;

        InternalChildren =
        [
            noteVisuals = new Container
            {
                // For simplicity in sizing and positioning
                // let's put the endpoints outside the main area
                Padding = new MarginPadding(-DrawableTap.CIRCLE_RADIUS),
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }
        ];
    }

    private void createParts(ISkin skin)
    {
        var texture = skin.GetTexture("sentakki/hold");
        var glowTexture = skin.GetTexture("sentakki/hold-glow");

        if (texture is null || glowTexture is null)
            return;

        var headTexture = texture.Crop(new RectangleF(0, 0, 1, 0.5f), Axes.Both);
        var headGlowTexture = glowTexture.Crop(new RectangleF(0, 0, 1, 0.5f), Axes.Both);

        var size = texture.Size;
        var glowSize = glowTexture.Size;

        var midRect = new RectangleF(new Vector2(0, (size.Y / 2) - 1), new Vector2(size.X, 2));
        var midGlowRect = new RectangleF(new Vector2(0, (glowSize.Y / 2) - 1), new Vector2(glowSize.X, 2));

        var middleTexture = texture.Crop(midRect, wrapModeT: WrapMode.Repeat);
        var middleGlowTexture = glowTexture.Crop(midGlowRect, wrapModeT: WrapMode.Repeat);

        var grid = new GridContainer()
        {
            RowDimensions = [
                new Dimension(GridSizeMode.Absolute, DrawableTap.CIRCLE_RADIUS),
                new Dimension(GridSizeMode.Distributed),
                new Dimension(GridSizeMode.Absolute, DrawableTap.CIRCLE_RADIUS)
            ],
            ColumnDimensions = [new Dimension(GridSizeMode.Distributed)],
            RelativeSizeAxes = Axes.Both,

            // The corner-to-corner diameter of the hexagon matches the diameter of a regular tap circle
            // On the vertical axis is is perfect, but there are no corners on the sides, so let's adjust the drawable size so it doesn't look weird
            Size = new Vector2(0.89f, 1.0f),

            Content = new Drawable[][]
            {
                [
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,

                        Children = [
                            headGlow = new Sprite
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                Texture = headGlowTexture,
                                // Counter adjust the scale of the glow to properly take into account drawable size
                                Scale = new Vector2(1.51f, 1.3f)
                            },
                            new Sprite()
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Texture = headTexture,
                            }
                        ]
                    },
                ],
            [
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,

                        Children = [
                            midGlow = new Sprite
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Texture = middleGlowTexture,
                                // Counter adjust the scale of the glow to properly take into account drawable size
                                Scale = new Vector2(1.5f, 1.0f)
                            },
                            new Sprite()
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Texture = middleTexture,
                            }
                        ]
                    },
                ],
            [
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Height = -1,

                        Children = [
                            tailGlow = new Sprite
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                Texture = headGlowTexture,
                                // Counter adjust the scale of the glow to properly take into account drawable size
                                Scale = new Vector2(1.51f, 1.3f)
                            },
                            new Sprite()
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Texture = headTexture,
                            }
                        ]
                    },
                ]
            }
        };

        noteVisuals.Child = grid;
    }

    private readonly IBindable<Color4> accentColour = new Bindable<Color4>();
    private readonly IBindable<bool> exBindable = new Bindable<bool>();

    [BackgroundDependencyLoader]
    private void load(DrawableHitObject? drawableObject, ISkinSource skin)
    {
        createParts(skin);

        if (drawableObject is not DrawableSentakkiHitObject dsho)
            return;

        accentColour.BindTo(drawableObject.AccentColour);
        accentColour.BindValueChanged(colour => Colour = colour.NewValue, true);

        exBindable.BindTo(dsho.ExBindable);
        exBindable.BindValueChanged(e =>
        {
            var colour = e.NewValue ? Color4.White : Color4.Black;

            headGlow.Colour = colour;
            midGlow.Colour = colour;
            tailGlow.Colour = colour;
        }, true);
    }
}
