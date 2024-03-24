using System;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableScorePaddingObject : DrawableHitObject<ScorePaddingObject>
    {
        public DrawableScorePaddingObject()
            : this(null)
        {
        }

        public DrawableScorePaddingObject(ScorePaddingObject? hitObject)
            : base(hitObject!)
        {
        }

        protected override JudgementResult CreateResult(Judgement judgement) => new SentakkiJudgementResult(HitObject, judgement);

        public new void ApplyResult(HitResult result)
        {
            var SentakkiJudgementResult = (SentakkiJudgementResult)Result;
            if (result == HitResult.Perfect)
            {
                SentakkiJudgementResult.Critical = true;
                result = Result.Judgement.MaxResult;
            }
            else
            {
                SentakkiJudgementResult.Critical = false;
            }

            base.ApplyResult(result);
        }
    }
}
