using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideTap : DrawableTap
    {
        private DrawableSlide slide;
        public DrawableSlideTap(SentakkiHitObject hitObject, DrawableSlide slide)
            : base(hitObject)
        {
            this.slide = slide;
        }

        protected override Drawable CreateTapRepresentation() => new SlideTapPiece();

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            double animTime = AnimationDuration.Value / 2 * GameplaySpeed;
            double animStart = HitObject.StartTime - animTime;
            using (BeginAbsoluteSequence(animStart, true))
            {
                (TapVisual as SlideTapPiece).Stars.Spin((slide.HitObject as IHasDuration).Duration, RotationDirection.CounterClockwise, 0).Loop();
            }
        }
    }
}
