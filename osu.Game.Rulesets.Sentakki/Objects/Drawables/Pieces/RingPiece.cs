using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class RingPiece : CompositeDrawable
    {
        public RingPiece(float thickness = 18, float outlineThickness = 1)
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            InternalChildren = new Drawable[]{
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    BorderThickness = thickness,
                    BorderColour = Color4.Gray,
                    Child = new Box
                    {
                        Alpha = 0,
                        AlwaysPresent = true,
                        RelativeSizeAxes = Axes.Both,
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(outlineThickness),
                    Child = new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Masking = true,
                        BorderThickness = thickness - (outlineThickness * 2),
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
