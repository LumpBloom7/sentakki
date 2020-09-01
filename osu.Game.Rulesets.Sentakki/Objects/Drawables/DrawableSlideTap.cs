using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using System.Linq;

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

            double spinDuration = ((slide.HitObject is Slide x) && x.SlideInfoList.Any()) ? x.SlideInfoList.Min(i => i.Duration) : 0;
            using (BeginAbsoluteSequence(animStart, true))
            {
                if (spinDuration > 0)
                    (TapVisual as SlideTapPiece).Stars.Spin(spinDuration, RotationDirection.CounterClockwise, 0).Loop();
            }
        }
    }
}
