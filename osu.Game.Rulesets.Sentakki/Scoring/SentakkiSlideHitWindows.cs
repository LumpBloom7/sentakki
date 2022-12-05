using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring
{
    public class SentakkiSlideHitWindows : SentakkiHitWindows
    {
        protected override DifficultyRange[] GetRanges() => new[]
        {
            new DifficultyRange(HitResult.Miss, 576, 576, 288),
            new DifficultyRange(HitResult.Ok, 576, 576, 288),
            new DifficultyRange(HitResult.Good, 416, 416, 208),
            new DifficultyRange(HitResult.Great, 288, 288, 144)
        };
    }
}
