using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring
{
    public class SentakkiSlideHitWindows : SentakkiHitWindows
    {
        protected override DifficultyRange[] GetRanges() => new DifficultyRange[]{
            new DifficultyRange(HitResult.Miss, 576, 576, 288),
            new DifficultyRange(HitResult.Good, 576, 576, 288),
            new DifficultyRange(HitResult.Great, 416, 416, 208),
            new DifficultyRange(HitResult.Perfect, 288, 288, 144)
        };
    }
}
