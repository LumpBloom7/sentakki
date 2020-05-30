using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring
{
    public class SentakkiScoreProcessor : ScoreProcessor
    {
        public override HitWindows CreateHitWindows() => new SentakkiHitWindows();
    }
}
