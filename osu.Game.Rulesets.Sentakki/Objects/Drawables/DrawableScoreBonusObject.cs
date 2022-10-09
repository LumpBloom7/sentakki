using System;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableScoreBonusObject : DrawableHitObject<ScoreBonusObject>
    {
        public DrawableScoreBonusObject() : this(null) { }

        public DrawableScoreBonusObject(ScoreBonusObject? hitObject)
            : base(hitObject!) { }

        public void TriggerResult() => ApplyResult(static r => r.Type = (Math.Abs(r.TimeOffset) < 16) ? r.Judgement.MaxResult : r.Judgement.MinResult);

        public new void ApplyResult(Action<JudgementResult> application)
        {
            if (!Result.HasResult)
                base.ApplyResult(application);
        }
    }
}
