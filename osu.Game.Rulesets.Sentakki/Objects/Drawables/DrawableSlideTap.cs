using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableSlideTap : DrawableTap
    {
        protected override Drawable CreateTapRepresentation() => new SlideTapPiece();

        protected new DrawableSlide ParentHitObject => (DrawableSlide)base.ParentHitObject;

        public DrawableSlideTap()
            : this(null)
        {
        }

        public DrawableSlideTap(SlideTap? hitObject)
            : base(hitObject)
        {
        }

        protected override void OnApply()
        {
            base.OnApply();

            if (TapVisual is SlideTapPiece note)
                note.SecondStar.Alpha = ParentHitObject.HitObject.SlideInfoList.Count > 1 ? 1 : 0;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            const double baseline_spin_duration = 250;

            var note = (SlideTapPiece)TapVisual;

            double spinDuration = baseline_spin_duration * (DrawableSentakkiRuleset?.GameplaySpeed ?? 1);

            if (ParentHitObject is DrawableSlide slide)
                spinDuration += slide.HitObject.SlideInfoList.FirstOrDefault()?.Duration ?? 1000;

            note.Stars.Spin(spinDuration, RotationDirection.Counterclockwise).Loop();
        }
    }
}
