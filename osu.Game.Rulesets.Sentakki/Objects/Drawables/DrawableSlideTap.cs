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
        private readonly DrawableSlide slide;
        public DrawableSlideTap(Tap hitObject, DrawableSlide slide)
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
            double spinDuration = ((Slide)slide.HitObject).SlideInfoList.FirstOrDefault().Duration;
            using (BeginAbsoluteSequence(animStart, true))
            {
                (TapVisual as SlideTapPiece).Stars.Spin(spinDuration, RotationDirection.CounterClockwise, 0).Loop();
            }
        }
    }
}
