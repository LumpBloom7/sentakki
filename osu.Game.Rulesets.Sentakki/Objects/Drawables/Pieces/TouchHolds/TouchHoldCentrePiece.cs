using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Touches;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds
{
    public partial class TouchHoldCentrePiece : CompositeDrawable
    {
        private readonly OsuColour colours = new OsuColour();
        public Container PieceContainer;

        public TouchHoldCentrePiece()
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Rotation = 45;
            Scale = new Vector2(80 / 90f);

            InternalChildren = new Drawable[]
            {
                PieceContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        createTouchShapeWith<TouchPieceShadow>(),
                        createTouchShapeWith<TouchHoldPiece>(),
                    }
                },
            };
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
                        Colour = colours.Red
                    },
                    new T
                    {
                        Anchor = Anchor.CentreRight,
                        Rotation = 90,
                        Colour = colours.Yellow,
                    },
                    new T
                    {
                        Anchor = Anchor.BottomCentre,
                        Rotation = 180,
                        Colour = colours.Green
                    },
                    new T
                    {
                        Anchor = Anchor.CentreLeft,
                        Rotation = 270,
                        Colour = colours.Blue,
                    },
                }
            };
    }
}
