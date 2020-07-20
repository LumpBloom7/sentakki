using osu.Game.Rulesets.Scoring;
using System.Diagnostics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideTailNode : DrawableSlideNode
    {
        public DrawableSlideTailNode(Slide.SlideNode node, DrawableSlide slideNote)
            : base(node, slideNote)
        {
        }

        // Needs work :)
        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (!userTriggered)
            {
                if (Slide.Auto && timeOffset > 0)
                {
                    HitPreviousNodes();
                    ApplyResult(r => r.Type = HitResult.Perfect);
                }

                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                {
                    HitPreviousNodes();
                    ApplyResult(r => r.Type = IsHittable ? HitResult.Good : HitResult.Miss);
                }
                return;
            }

            if (!IsHittable) return;

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (timeOffset < 0 && result == HitResult.None)
                result = HitResult.Meh;

            ApplyResult(r => r.Type = result);
            HitPreviousNodes();
        }
    }
}
