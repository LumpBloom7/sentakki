using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Skinning;
using osu.Game.Rulesets.Sentakki.Skinning.Default.Slides;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideTap : DrawableTap
    {
        protected override SentakkiSkinComponents TapPieceComponent => SentakkiSkinComponents.SlideStar;
        protected override Type fallbackPieceType => typeof(SlideTapPiece);

        public DrawableSlideTap() : this(null) { }
        public DrawableSlideTap(SlideTap? hitObject)
            : base(hitObject) { }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            var note = (SlideTapPiece)TapVisual.Drawable;

            double spinDuration = 0;

            if (ParentHitObject is DrawableSlide slide)
            {
                spinDuration = ((Slide)slide.HitObject).SlideInfoList.FirstOrDefault().Duration;
                if (slide.SlideBodies.Count > 1)
                    note.SecondStar.Alpha = 1;
                else
                    note.SecondStar.Alpha = 0;
            }

            note.Stars.Spin(spinDuration, RotationDirection.Counterclockwise, 0).Loop();
        }
    }
}
