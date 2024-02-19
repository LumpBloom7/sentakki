using System;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableScoreBonusObject : DrawableHitObject<ScoreBonusObject>
    {
        public DrawableScoreBonusObject()
            : this(null)
        {
        }

        public DrawableScoreBonusObject(ScoreBonusObject? hitObject)
            : base(hitObject!)
        {
        }

        public void TriggerResult()
        {
            ApplyResult((r, dho) =>
            {
                double timeOffset = Math.Abs(Time.Current - HitObject.StartTime);
                bool isCrit = r.HitObject.HitWindows.ResultFor(timeOffset) == HitResult.Perfect;
                r.Type = isCrit ? r.Judgement.MaxResult : r.Judgement.MinResult;
            });
        }
    }
}
