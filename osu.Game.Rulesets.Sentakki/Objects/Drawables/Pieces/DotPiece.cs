using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public partial class DotPiece : CompositeDrawable
    {
        public DotPiece(float outlineThickness = 2, bool squared = false)
            : this(new Vector2(SentakkiPlayfield.DOTSIZE), outlineThickness, squared)
        {
        }

        public DotPiece(Vector2 size, float outlineThickness = 2, bool squared = false)
        {
            Size = size;
            Vector2 innerDotSize = size - new Vector2(outlineThickness * 2);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Masking = true;
            CornerExponent = squared ? 2.5f : 2f;
            CornerRadius = Math.Min(size.X, size.Y) / (squared ? 4 : 2);
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Gray,
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    CornerExponent = squared ? 2.5f : 2f,
                    CornerRadius = Math.Min(innerDotSize.X, innerDotSize.Y) / (squared ? 4 : 2),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = innerDotSize,
                    Masking = true,
                    Child = new Box
                    {
                        Colour = Color4.White,
                        RelativeSizeAxes = Axes.Both,
                    },
                }
            };
        }
    }
}
