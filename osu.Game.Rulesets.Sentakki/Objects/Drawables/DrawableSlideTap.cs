using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Utils;
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

        private double spinDurationModifier = 0;

        protected override void OnApply()
        {
            base.OnApply();

            if (TapVisual is SlideTapPiece note)
                note.SecondStar.Alpha = ParentHitObject.HitObject.SlideInfoList.Count > 1 ? 1 : 0;

            if (ParentHitObject is DrawableSlide slide)
                spinDurationModifier = slide.HitObject.SlideInfoList.MinBy(si => si.Duration)?.Duration ?? 0;
            else
                spinDurationModifier = 0;
        }

        protected override void UpdateNoteVisuals()
        {
            base.UpdateNoteVisuals();

            const double baseline_spin_duration = 250;

            var note = (SlideTapPiece)TapVisual;

            double animStartTime = HitObject.StartTime - AnimationDuration.Value;

            float spinProgress = Interpolation.ValueAt(Time.Current, 0f, 360, HitObject.StartTime - AnimationDuration.Value, animStartTime + baseline_spin_duration + spinDurationModifier);

            note.Stars.Rotation = spinProgress;
        }
    }
}
