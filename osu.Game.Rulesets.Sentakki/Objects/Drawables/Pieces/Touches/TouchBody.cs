using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Touches
{
    public partial class TouchBody : Container
    {
        public Container BorderContainer;
        public Container PieceContainer;

        public TouchBody()
        {
            Size = new Vector2(130);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Alpha = 0;

            InternalChildren = new Drawable[]
            {
                PieceContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        createTouchShapeWith<TouchGlowPiece>(), // Meant for the drop shadow/glow
                        createTouchShapeWith<TouchPiece>(),
                        new DotPiece()
                    }
                },
                BorderContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(100),
                    CornerRadius = 25f,
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
            };
        }

        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [BackgroundDependencyLoader(true)]
        private void load(DrawableHitObject drawableObject)
        {
            if (drawableObject is null) return;

            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour => PieceContainer.Colour = colour.NewValue, true);
        }

        // Creates the touch shape using the provided drawable as each of the 4 quarters
        private Drawable createTouchShapeWith<T>() where T : Drawable, new()
            => new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]{
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
                }
            };
    }
}
