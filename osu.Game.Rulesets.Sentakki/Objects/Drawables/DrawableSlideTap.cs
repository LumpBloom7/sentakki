using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideTap : DrawableTap
    {
        protected override Drawable CreateTapRepresentation() => new SlideTapPiece();

        private DrawableSlide slide;

        public DrawableSlideTap() : this(null) { }
        public DrawableSlideTap(SlideTap hitObject)
            : base(hitObject) { }

        protected override void OnParentReceived(DrawableHitObject parent)
        {
            base.OnParentReceived(parent);
            slide = (DrawableSlide)parent;
            AccentColour.BindTo(slide.AccentColour);
        }

        protected override void OnFree()
        {
            base.OnFree();
            AccentColour.UnbindFrom(slide.AccentColour);
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            var note = TapVisual as SlideTapPiece;

            if (slide.SlideBodies.Count > 1)
                note.SecondStar.Alpha = 1;
            else
                note.SecondStar.Alpha = 0;

            double spinDuration = ((Slide)slide.HitObject).SlideInfoList.FirstOrDefault().Duration;
            note.Stars.Spin(spinDuration, RotationDirection.CounterClockwise, 0).Loop();
        }
    }
}
