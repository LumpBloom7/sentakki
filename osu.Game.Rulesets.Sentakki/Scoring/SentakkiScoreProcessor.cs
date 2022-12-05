using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring
{
    public partial class SentakkiScoreProcessor : ScoreProcessor
    {
        public SentakkiScoreProcessor(SentakkiRuleset ruleset)
            : base(ruleset)
        {
        }

        protected override double DefaultAccuracyPortion => 0.9;
        protected override double DefaultComboPortion => 0.1;
    }
}
