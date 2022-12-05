using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public partial class RingPiece : CircularContainer
    {
        private const float outline_thickness = 2;

        public RingPiece(float thickness = 18)
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Masking = true;
            BorderThickness = thickness;
            BorderColour = Color4.Gray;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    AlwaysPresent = true,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(outline_thickness),
                    Child = new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Masking = true,
                        BorderThickness = thickness - (outline_thickness * 2),
                        BorderColour = Color4.White,
                        Child = new Box
                        {
                            Alpha = 0,
                            AlwaysPresent = true,
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                }
            };
        }
    }
}
