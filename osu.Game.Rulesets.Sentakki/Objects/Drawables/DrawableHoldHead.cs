using System.Diagnostics;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableHoldHead : DrawableSentakkiLanedHitObject
    {
        protected override bool PlayBreakSample => false;

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
                    ApplyResult(r => r.Type = HitResult.Great);

                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = HitResult.Miss);
                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None || (result == HitResult.Miss && Time.Current < HitObject.StartTime))
                return;

            ApplyResult(r => r.Type = result);
        }

        public void UpdateResult() => base.UpdateResult(true);

        public void MissForcefully() => ApplyResult(r => r.Type = HitResult.Miss);
    }
}
