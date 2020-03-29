using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables.Pieces
{
    class CirclePiece : CircularContainer
    {
        public CircularContainer outline;
        public CircularContainer innerOutline;
        public CircularContainer coloredCircle;
        public CirclePiece()
        {
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(1f);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                innerOutline = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 16.35f,
                    BorderColour = Color4.Black,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha= 0,
                        AlwaysPresent = true
                    }
                },
                coloredCircle = new CircularContainer
                {
                    Masking = true,
                    BorderThickness = 15,
                    RelativeSizeAxes = Axes.Both,
                    BorderColour = Color4.White,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha= 0,
                        AlwaysPresent = true
                    }
                },
                outline = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 2,
                    BorderColour = Color4.Black,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha= 0,
                        AlwaysPresent = true
                    }
                },

            };
        }
    }
}
