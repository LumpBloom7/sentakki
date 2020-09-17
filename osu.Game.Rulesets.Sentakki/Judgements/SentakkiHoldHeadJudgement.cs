using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Judgements
{
    public class SentakkiHoldHeadJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.Great;
        public override bool AffectsCombo => false;

        protected override int NumericResultFor(HitResult result) => 0;

        protected override double HealthIncreaseFor(HitResult result) => 0;
    }
}
