using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring
{
    public class SentakkiTouchHitWindows : SentakkiHitWindows
    {
        protected override DifficultyRange[] GetRanges() => new DifficultyRange[]{
            new DifficultyRange(HitResult.Miss, 288, 288, 288),
            new DifficultyRange(HitResult.Good, 288, 288, 288),
            new DifficultyRange(HitResult.Great, 240, 240, 240),
            new DifficultyRange(HitResult.Perfect, 192, 192, 192),
        };
    }
}
