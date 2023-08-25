using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring
{
    public class SentakkiTouchHitWindows : SentakkiHitWindows
    {
        protected override DifficultyRange[] GetRanges() => new[]
        {
            SimpleDifficultyRange(HitResult.Miss, 18 * timing_unit),
            SimpleDifficultyRange(HitResult.Ok, 18 * timing_unit),
            SimpleDifficultyRange(HitResult.Good, 15 * timing_unit),
            SimpleDifficultyRange(HitResult.Great, 12 * timing_unit),
            SimpleDifficultyRange(HitResult.Perfect, 9 * timing_unit)
        };
    }
}
