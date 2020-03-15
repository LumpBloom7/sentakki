using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables.Pieces
{
    public class FlashPiece : Container
    {
        public FlashPiece()
        {
            Size = new Vector2(80);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Blending = BlendingParameters.Additive;
            Alpha = 0;

            Child = new CircularContainer
            {
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both
                }
            };
        }
    }
}
