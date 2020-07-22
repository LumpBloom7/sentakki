using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideTap : DrawableTap
    {
        public DrawableSlideTap(SentakkiHitObject hitObject)
            : base(hitObject)
        { }

        protected override Drawable CreateTapRepresentation() => new SlideTapPiece();
    }
}
