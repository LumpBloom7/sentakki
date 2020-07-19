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
            bool previousHit = Slide.SlideNodes.IndexOf(this) >= 2 && Slide.SlideNodes[Slide.SlideNodes.IndexOf(this) - 2].IsHit;
            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = previousHit ? HitResult.Good : HitResult.Miss);
                return;
            }

            if (!previousHit) return;

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                return;

            ApplyResult(r => r.Type = result);
        }
    }
}
