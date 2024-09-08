using System;
using osu.Game.Rulesets.Scoring;
namespace osu.Game.Rulesets.Sentakki.Scoring
{
    public partial class SentakkiScoreProcessor : ScoreProcessor
    {
        public SentakkiScoreProcessor(SentakkiRuleset ruleset)
            : base(ruleset)
        {
        }

        protected override double ComputeTotalScore(double comboProgress, double accuracyProgress, double bonusPortion)
        {
            return (10000 * comboProgress)
                   + (990000 * Math.Pow(Accuracy.Value, 2 + (2 * Accuracy.Value)) * accuracyProgress)
                   + bonusPortion;
        }

        public override int GetBaseScoreForResult(HitResult result)
        {
            if (result == HitResult.Perfect)
                return 305;

            return base.GetBaseScoreForResult(result);
        }
    }
}
