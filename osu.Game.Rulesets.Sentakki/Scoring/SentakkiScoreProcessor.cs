using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
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
            return (150000 * comboProgress)
                + (850000 * Math.Pow(Accuracy.Value, 2 + (2 * Accuracy.Value)) * accuracyProgress)
                + bonusPortion;
        }

        public override int GetBaseScoreForResult(HitResult result)
        {
            if (result == HitResult.Perfect)
                return 305;

            return base.GetBaseScoreForResult(result);
        }

        public override ScoreRank RankFromScore(double accuracy, IReadOnlyDictionary<HitResult, int> results)
        {
            bool anyImperfect =
                results.GetValueOrDefault(HitResult.Good) > 0
                || results.GetValueOrDefault(HitResult.Ok) > 0
                || results.GetValueOrDefault(HitResult.Meh) > 0
                || results.GetValueOrDefault(HitResult.Miss) > 0;

            return !anyImperfect ? ScoreRank.X : base.RankFromScore(accuracy, results);
        }
    }
}
