using osu.Game.Rulesets.Scoring;
using System.Diagnostics;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables
{
    public class DrawableHoldTail : DrawableMaimaiHitObject
    {
        /// <summary>
        /// Lenience of release hit windows. This is to make cases where the hold note release
        /// is timed alongside presses of other hit objects less awkward.
        /// Todo: This shouldn't exist for non-LegacyBeatmapDecoder beatmaps
        /// </summary>
        private const double release_window_lenience = 3;

        private readonly DrawableHold holdNote;

        public DrawableHoldTail(DrawableHold holdNote)
            : base((holdNote.HitObject as Hold).Tail)
        {
            this.holdNote = holdNote;
        }

        public void UpdateResult() => base.UpdateResult(true);

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            // Factor in the release lenience
            timeOffset /= release_window_lenience;

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    if (Time.Current > HitObject.StartTime)
                        ApplyResult(r => r.Type = HitResult.Ok);
                    else
                        ApplyResult(r => r.Type = HitResult.Miss);

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None || (result == HitResult.Miss && timeOffset < 0))
                return;

            ApplyResult(r =>
            {
                // If the head wasn't hit or the hold note was broken, cap the max score to Meh.
                if (result > HitResult.Ok && !holdNote.Head.IsHit)
                    result = HitResult.Ok;

                r.Type = result;
            });
        }
    }
}
