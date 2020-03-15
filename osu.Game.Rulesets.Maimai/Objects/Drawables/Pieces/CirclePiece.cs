using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables.Pieces
{
    class CirclePiece : CircularContainer
    {
        public CircularContainer InnerCircle;
        public CirclePiece()
        {
            Size = new Vector2(80);
            Children = new Drawable[]
            {
                InnerCircle = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 15,
                    BorderColour = Color4.White,
                    Child = new Box
                    {
                        AlwaysPresent = true,
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both
                    }
                },
                new CircularContainer{
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 3,
                    BorderColour = Color4.Black,
                    Child = new Box
                    {
                        AlwaysPresent = true,
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both
                    }
                }

            };
        }


    }
}
