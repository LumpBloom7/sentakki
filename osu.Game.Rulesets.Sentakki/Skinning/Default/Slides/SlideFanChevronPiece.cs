using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default.Slides
{
    public class SlideFanChevronPiece : CompositeDrawable
    {
        public SlideFanChevronPiece(float progress)
        {
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            AutoSizeAxes = Axes.Both;

            float chevHeight = 16 + (10 * progress);
            float chevWidth = 6 + (210 * progress);

            InternalChildren = new Drawable[]
            {
                // Outlines
                new Container
                {
                    X = 2.5f,
                    Masking = true,
                    CornerRadius = chevHeight/4,
                    CornerExponent = 2.5f,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomRight,
                    Rotation = 22.5f,
                    Width = chevWidth,
                    Height = chevHeight,
                    Child = new Box{
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Gray
                    },
                },
                new Container
                {
                    X = -2.5f,
                    Masking = true,
                    CornerRadius = chevHeight/4,
                    CornerExponent = 2.5f,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomLeft,
                    Rotation = -22.5f,
                    Width = chevWidth,
                    Height = chevHeight,
                    Child = new Box{
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Gray
                    },
                },
                // Inners
                new Container
                {
                    X = 2.5f,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomRight,
                    Size = new Vector2(chevWidth, chevHeight),
                    Rotation = 22.5f,
                    Padding = new MarginPadding(2),
                    Child = new Container{
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,

                        CornerRadius = (chevHeight-4)/4,
                        CornerExponent = 2.5f,
                        Colour = Color4.White,
                        Child = new Box{
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White
                        }
                    },
                },
                new Container
                {
                    X = -2.5f,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomLeft,
                    Size = new Vector2(chevWidth, chevHeight),
                    Rotation = -22.5f,
                    Padding = new MarginPadding(2),
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = (chevHeight-4)/4,
                        CornerExponent = 2.5f,
                        Child = new Box{
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White
                        }
                    },
                },
            };
        }
    }
}
