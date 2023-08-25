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
            ApplyResult(static r =>
                {
                    bool isCrit = r.HitObject.HitWindows.ResultFor(r.TimeOffset) == HitResult.Perfect;

                    r.Type = isCrit ? r.Judgement.MaxResult : r.Judgement.MinResult;
                }
            );
        }

        public new void ApplyResult(Action<JudgementResult> application)
        {
            if (!Result.HasResult)
                base.ApplyResult(application);
        }
    }
}
