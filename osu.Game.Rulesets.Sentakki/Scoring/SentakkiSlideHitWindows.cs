using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring
{
    public class SentakkiSlideHitWindows : SentakkiHitWindows
    {
        protected override DifficultyRange[] GetRanges() => new[]
        {
            SimpleDifficultyRange(HitResult.Miss, 36 * timing_unit),
            SimpleDifficultyRange(HitResult.Ok, 36 * timing_unit),
            SimpleDifficultyRange(HitResult.Good, 26 * timing_unit),
            SimpleDifficultyRange(HitResult.Great, 14 * timing_unit),
            SimpleDifficultyRange(HitResult.Perfect, 14 * timing_unit),
        };
    }
}
