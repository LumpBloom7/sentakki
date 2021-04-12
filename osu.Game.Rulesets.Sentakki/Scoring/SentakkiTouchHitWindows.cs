using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring
{
    public class SentakkiTouchHitWindows : SentakkiHitWindows
    {
        protected override DifficultyRange[] GetRanges() => new DifficultyRange[]{
            new DifficultyRange(HitResult.Miss, 288, 288, 144),
            new DifficultyRange(HitResult.Ok, 288, 288, 144),
            new DifficultyRange(HitResult.Good, 240, 240, 120),
            new DifficultyRange(HitResult.Great, 192, 192, 96),
        };
    }
}
