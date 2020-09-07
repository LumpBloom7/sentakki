using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring
{
    public class SentakkiScoreProcessor : ScoreProcessor
    {
        protected override double DefaultAccuracyPortion => 0.9;
        protected override double DefaultComboPortion => 0.1;

        public override HitWindows CreateHitWindows() => new SentakkiHitWindows();
    }
}
