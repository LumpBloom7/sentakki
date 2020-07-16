using osu.Game.Rulesets.Scoring;
using System.Diagnostics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableHoldTail : DrawableSentakkiHitObject
    {
        private readonly DrawableHold holdNote;

        public DrawableHoldTail(DrawableHold holdNote)
            : base((holdNote.HitObject as Hold).Tail)
        {
            this.holdNote = holdNote;
        }

        public void UpdateResult() => base.UpdateResult(true);

        protected override bool PlayBreakSample => false;

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (holdNote.Auto && timeOffset > 0)
                    ApplyResult(r => r.Type = HitResult.Perfect);

                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    if (holdNote.IsHitting.Value)
                        ApplyResult(r => r.Type = HitResult.Ok);
                    else
                        ApplyResult(r => r.Type = HitResult.Miss);

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None || (result == HitResult.Miss && Time.Current < HitObject.StartTime))
                return;

            ApplyResult(r =>
            {
                // If the head wasn't hit or the hold note was broken, cap the max score to Meh.
                if (result > HitResult.Meh && !holdNote.Head.IsHit)
                    result = HitResult.Meh;

                r.Type = result;
            });
        }
    }
}
