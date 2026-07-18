using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
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

        if (texture is null)
            return;

        var headTexture = texture.Crop(new RectangleF(0, 0, 1, 0.5f), Axes.Both);

        var size = texture.Size;

        var midRect = new RectangleF(new Vector2(0, (size.Y / 2) - 1), new Vector2(size.X, 2));

        var middleTexture = texture.Crop(midRect, wrapModeT: Framework.Graphics.Textures.WrapMode.Repeat);

        var grid = new GridContainer()
        {
            RowDimensions = [
                new Dimension(GridSizeMode.Absolute, DrawableTap.CIRCLE_RADIUS),
                new Dimension(GridSizeMode.Distributed),
                new Dimension(GridSizeMode.Absolute, DrawableTap.CIRCLE_RADIUS)
            ],
            ColumnDimensions = [new Dimension(GridSizeMode.Distributed)],
            RelativeSizeAxes = Axes.Both,

            Size = new Vector2(0.89f, 1.0f),

            Content = new Drawable[][]
            {
                [
                    new Sprite()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Texture = headTexture,
                    }
                ],
            [
                    new Sprite()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Texture = middleTexture,
                    }
                ],
            [
                    new Sprite()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = -1,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Texture = headTexture,
                    }
                ]
            }
        };

        noteVisuals.Child = grid;
    }

    private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

    [BackgroundDependencyLoader]
    private void load(DrawableHitObject? drawableObject, ISkinSource skin)
    {
        createParts(skin);

        if (drawableObject is null)
            return;

        accentColour.BindTo(drawableObject.AccentColour);
        accentColour.BindValueChanged(colour => Colour = colour.NewValue, true);
    }
}
