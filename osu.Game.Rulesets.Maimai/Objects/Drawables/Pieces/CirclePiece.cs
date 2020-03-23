using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables.Pieces
{
    class CirclePiece : CircularContainer
    {
        public CirclePiece()
        {
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(1f);
            Masking = true;
            BorderThickness = 15;
            BorderColour = Color4.White;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha= 0,
                    AlwaysPresent = true
                },
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 3,
                    BorderColour = Color4.Black,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha= 0,
                        AlwaysPresent = true
                    }
                }
            };
        }
    }
}
