using osu.Game.Rulesets.Scoring;
using System.Diagnostics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableHoldHead : DrawableSentakkiHitObject
    {
        private DrawableHold hold;
        public DrawableHoldHead(DrawableHold holdNote)
            : base((holdNote.HitObject as Hold).Head)
        {
            hold = holdNote;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (hold.Auto && timeOffset > 0)
                    ApplyResult(r => r.Type = HitResult.Perfect);

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
