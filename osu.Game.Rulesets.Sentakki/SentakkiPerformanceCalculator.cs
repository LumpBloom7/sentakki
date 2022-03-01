using osu.Game.Rulesets.Difficulty;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Sentakki
{
    public class SentakkiPerformanceCalculator : PerformanceCalculator
    {
        public SentakkiPerformanceCalculator(SentakkiRuleset ruleset, DifficultyAttributes attributes, ScoreInfo score)
            : base(ruleset, attributes, score)
        {
        }

        public override PerformanceAttributes Calculate() => new PerformanceAttributes { Total = 0 };
    }
}
