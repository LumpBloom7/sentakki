using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring;

public class SentakkiSlideHitWindows : SentakkiHitWindows
{
    private static readonly DifficultyRange[] default_ranges = {
        SimpleDifficultyRange(HitResult.Miss, 36 * timing_unit),
        SimpleDifficultyRange(HitResult.Ok, 36 * timing_unit),
        SimpleDifficultyRange(HitResult.Good, 26 * timing_unit),
        SimpleDifficultyRange(HitResult.Great, 14 * timing_unit),
        SimpleDifficultyRange(HitResult.Perfect, 14 * timing_unit),
    };

    private static readonly DifficultyRange[] maji_ranges = {
        SimpleDifficultyRange(HitResult.Miss, 26 * timing_unit),
        SimpleDifficultyRange(HitResult.Ok, 26 * timing_unit),
        SimpleDifficultyRange(HitResult.Good, 14 * timing_unit),
        SimpleDifficultyRange(HitResult.Great, 14 * timing_unit),
        SimpleDifficultyRange(HitResult.Perfect, 14 * timing_unit),
    };

    protected override DifficultyRange[] GetDefaultRanges() => default_ranges;
    protected override DifficultyRange[] GetMajiRanges() => maji_ranges;
}
