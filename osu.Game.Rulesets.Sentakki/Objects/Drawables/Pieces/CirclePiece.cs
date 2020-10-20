using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class CirclePiece : CircularContainer
    {
        public CirclePiece()
        {
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(1);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                new RingPiece(),
                new DotPiece()
            };
        }
    }
}
