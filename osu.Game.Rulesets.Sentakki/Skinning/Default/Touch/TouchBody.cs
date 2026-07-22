using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.Skinning.Common;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default.Touch;

public partial class TouchBody : CompositeDrawable, ITouchBody
{
    public Drawable Border => BorderContainer;

    public Container BorderContainer;
    public Container PieceContainer;

    public TouchBody()
    {
        RelativeSizeAxes = Axes.Both;
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        InternalChildren =
        [
            PieceContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children =
                [
                    createTouchShape<TouchPieceShadow>(),
                    createTouchShape<TouchPiece>(),
                    new DotPiece()
                ]
            },
            BorderContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Scale = new Vector2(10f / 9f),
                CornerRadius = 22.5f,
                CornerExponent = 2.5f,
                Masking = true,
                BorderThickness = 10,
                BorderColour = Color4.White,
                Alpha = 0,
                Child = new Box
                {
                    Alpha = 0,
                    AlwaysPresent = true,
                    RelativeSizeAxes = Axes.Both
                }
            },
        ];
    }

    private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

    [BackgroundDependencyLoader]
    private void load(DrawableHitObject? drawableObject)
    {
        if (drawableObject is null)
            return;

        accentColour.BindTo(drawableObject.AccentColour);
        accentColour.BindValueChanged(colour => PieceContainer.Colour = colour.NewValue, true);
    }

    // Creates the touch shape using the provided drawable as each of the 4 quarters
    private Drawable createTouchShape<T>() where T : Drawable, new()
        => new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Children =
            [
                new T
                {
                    Anchor = Anchor.TopCentre,
                },
                new T
                {
                    Anchor = Anchor.BottomCentre,
                    Rotation = 180
                },
                new T
                {
                    Anchor = Anchor.CentreLeft,
                    Rotation = 270
                },
                new T
                {
                    Anchor = Anchor.CentreRight,
                    Rotation = 90
                },
            ]
        };
}
