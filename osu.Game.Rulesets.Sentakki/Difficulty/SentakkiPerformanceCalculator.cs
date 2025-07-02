using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;


namespace osu.Game.Rulesets.Sentakki.Difficulty
{
    public class SentakkiPerformanceCalculator : PerformanceCalculator
    {
        public SentakkiPerformanceCalculator()
            : base(new SentakkiRuleset()) { }

        protected override PerformanceAttributes CreatePerformanceAttributes(ScoreInfo score, DifficultyAttributes attributes)
        {
            var sentakkiAttributes = (SentakkiDifficultyAttributes)attributes;
            double accuracy = score.Accuracy;
            int countMiss = score.Statistics.GetValueOrDefault(HitResult.Miss);

            double baseValue = Math.Pow((5.0f * Math.Max(1.0f, sentakkiAttributes.StarRating / 0.0049f)) - 4.0f, 2.0f) / 100000.0f;
            double value = baseValue;
            double lengthBonus = 0.95 + ((0.3 * Math.Min(1.0, sentakkiAttributes.MaxCombo / 2500.0)) + (sentakkiAttributes.MaxCombo > 2500 ? Math.Log10(sentakkiAttributes.MaxCombo / 2500.0f) * 0.475f : 0.0f));
            value *= lengthBonus;
            value *= Math.Pow(0.97, countMiss);
            if (sentakkiAttributes.MaxCombo > 0)
                value *= Math.Min(Math.Pow(score.MaxCombo, 0.35f) / Math.Pow(sentakkiAttributes.MaxCombo, 0.35f), 1.0f);
            value *= Math.Pow(accuracy, 5.5);
            //Until difficulty calculation is implemented,
            //multiply combo progress instead of strain.
            //(I think it is strange that the first combo gets 600pp.)
            double comboProgress = (double)sentakkiAttributes.MaxCombo / score.GetMaximumAchievableCombo();
            double totalValue = value * comboProgress;

            return new SentakkiPerformanceAttributes
            {
                Base_PP = baseValue,
                Length_Bonus = lengthBonus,
                Total = totalValue
            };
        }
    }
}
