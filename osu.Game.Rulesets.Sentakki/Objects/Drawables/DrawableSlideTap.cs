using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Skinning;
using osu.Game.Rulesets.Sentakki.Skinning.Default.Slides;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideTap : DrawableTap
    {
        protected override Drawable CreateTapVisual() => new SlideTapPiece()
        {
            Scale = new Vector2(0f),
            Position = new Vector2(0, -SentakkiPlayfield.NOTESTARTDISTANCE),
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        };

        public DrawableSlideTap() : this(null) { }
        public DrawableSlideTap(SlideTap? hitObject)
            : base(hitObject) { }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            var note = (ISlideTapPiece)TapVisual;

            double spinDuration = 0;

            if (ParentHitObject is DrawableSlide slide)
            {
                spinDuration = ((Slide)slide.HitObject).SlideInfoList.FirstOrDefault().Duration;
                if (slide.SlideBodies.Count > 1)
                    note.Stars[1].Alpha = 1;
                else
                    note.Stars[1].Alpha = 0;
            }

            note.Stars.Spin(spinDuration, RotationDirection.Counterclockwise, 0).Loop();
        }
    }
}
