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
    private Drawable glowLayer = null!;
    private Drawable mainBody = null!;

    private readonly IBindable<Color4> accentColour = new Bindable<Color4>();
    private readonly IBindable<bool> exBindable = new Bindable<bool>();

    public override Quad ScreenSpaceDrawQuad => mainBody.ScreenSpaceDrawQuad;

    public LegacyHoldBody()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.Both;
    }

    [BackgroundDependencyLoader]
    private void load(DrawableHitObject? drawableObject, ISkinSource skin)
    {
        InternalChild = new Container
        {
            // For simplicity in sizing and positioning
            // let's put the endpoints outside the main area
            Padding = new MarginPadding(-DrawableTap.CIRCLE_RADIUS),
            RelativeSizeAxes = Axes.Both,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                Children = [
                    glowLayer = createGlow(skin),
                    mainBody = createMainBody(skin),
                ]
            }
        };

        if (drawableObject is not DrawableSentakkiHitObject dsho)
            return;

        accentColour.BindTo(drawableObject.AccentColour);
        accentColour.BindValueChanged(colour => Colour = colour.NewValue, true);

        exBindable.BindTo(dsho.ExBindable);
        exBindable.BindValueChanged(e => glowLayer.Colour = e.NewValue ? Color4.White : Color4.Black, true);
    }

    private static GridContainer createMainBody(ISkin skin)
    {
        var texture = skin.GetTexture("sentakki/hold");

        var headCapTexture = texture?.Crop(new RectangleF(0, 0, 1, 0.5f), Axes.Both);
        var tailCapTexture = texture?.Crop(new RectangleF(0, 0.5f, 1, 0.5f), Axes.Both);
        var bodyTexture = texture?.Crop(new RectangleF(0, (texture.Size.Y / 2) - 1, texture.Size.X, 2), wrapModeT: WrapMode.Repeat);

        var grid = new GridContainer()
        {
            RowDimensions = [
                new Dimension(GridSizeMode.Absolute, DrawableTap.CIRCLE_RADIUS),
                new Dimension(GridSizeMode.Distributed),
                new Dimension(GridSizeMode.Absolute, DrawableTap.CIRCLE_RADIUS)
            ],
            ColumnDimensions = [new Dimension(GridSizeMode.Distributed)],
            RelativeSizeAxes = Axes.Both,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,

            // The corner-to-corner diameter of the hexagon matches the diameter of a regular tap circle
            // On the vertical axis is is perfect, but there are no corners on the sides, so let's adjust the drawable size so it doesn't look weird
            Size = new Vector2(0.89f, 1.0f),

            Content = new Drawable[][]
            {
                    [new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Texture = headCapTexture,
                    }],
                    [new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Texture = bodyTexture,
                    }],
                    [new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Texture = tailCapTexture,
                    }]
            }
        };

        return grid;
    }

    private static GridContainer createGlow(ISkin skin)
    {
        var texture = skin.GetTexture("sentakki/glow/hold");

        var headCapTexture = texture?.Crop(new RectangleF(0, 0, 1, 0.5f), Axes.Both);
        var tailCapTexture = texture?.Crop(new RectangleF(0, 0.5f, 1, 0.5f), Axes.Both);
        var bodyTexture = texture?.Crop(new RectangleF(0, (texture.Size.Y / 2) - 1, texture.Size.X, 2), wrapModeT: WrapMode.Repeat);

        var grid = new GridContainer()
        {
            RowDimensions = [
                new Dimension(GridSizeMode.Absolute, DrawableTap.CIRCLE_RADIUS),
                new Dimension(GridSizeMode.Distributed),
                new Dimension(GridSizeMode.Absolute, DrawableTap.CIRCLE_RADIUS)
            ],
            ColumnDimensions = [new Dimension(GridSizeMode.Distributed)],
            RelativeSizeAxes = Axes.Both,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,

            // The corner-to-corner diameter of the hexagon matches the diameter of a regular tap circle
            // On the vertical axis is is perfect, but there are no corners on the sides, so let's adjust the drawable size so it doesn't look weird
            Size = new Vector2(0.89f, 1.0f),

            Content = new Drawable[][]
            {
                [new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Texture = headCapTexture,
                    Scale = new Vector2(1.5f),
                }],
                [new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = bodyTexture,
                    // The body doesn't have a top and bottom edge, don't scale the vertical axis so to avoid overlapping shadows
                    Scale = new Vector2(1.5f, 1.0f),
                }],
                [new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Texture = tailCapTexture,
                    Scale = new Vector2(1.5f),
                }]
            }
        };

        return grid;
    }
}
