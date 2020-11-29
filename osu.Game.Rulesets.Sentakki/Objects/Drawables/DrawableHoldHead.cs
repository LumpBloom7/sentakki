using System.Diagnostics;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableHoldHead : DrawableSentakkiLanedHitObject
    {
        public DrawableHoldHead() : this(null) { }

        public DrawableHoldHead(Hold.HoldHead hitObject)
            : base(hitObject)
        {
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (Auto && timeOffset > 0)
                    ApplyResult(r => r.Type = r.Judgement.MaxResult);

                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = r.Judgement.MinResult);
                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                return;

            ApplyResult(r => r.Type = result);
        }

        public void UpdateResult() => UpdateResult(true);

        public void MissForcefully() => ApplyResult(r => r.Type = HitResult.Miss);
    }
}
