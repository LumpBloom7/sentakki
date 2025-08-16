using osu.Game.Rulesets.Difficulty;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Sentakki;

public class SentakkiPerformanceCalculator : PerformanceCalculator
{
    public SentakkiPerformanceCalculator(SentakkiRuleset ruleset)
        : base(ruleset)
    {
    }

    // TODO: Create an actual performance calculator
    protected override PerformanceAttributes CreatePerformanceAttributes(ScoreInfo score, DifficultyAttributes attributes) => new PerformanceAttributes { Total = 0 };
}
