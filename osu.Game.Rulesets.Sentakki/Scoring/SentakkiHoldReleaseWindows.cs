using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring;

public class SentakkiHoldReleaseWindows : SentakkiHitWindows
{
    private static readonly DifficultyRange[] default_ranges =
    {
        SimpleDifficultyRange(HitResult.Miss, 12 * timing_unit),
        SimpleDifficultyRange(HitResult.Ok, 12 * timing_unit),
        SimpleDifficultyRange(HitResult.Good, 9 * timing_unit),
        SimpleDifficultyRange(HitResult.Great, 6 * timing_unit),
        SimpleDifficultyRange(HitResult.Perfect, 3 * timing_unit),
    };

    private static readonly DifficultyRange[] maji_ranges =
    {
        SimpleDifficultyRange(HitResult.Miss, 9 * timing_unit),
        SimpleDifficultyRange(HitResult.Ok, 9 * timing_unit),
        SimpleDifficultyRange(HitResult.Good, 6 * timing_unit),
        SimpleDifficultyRange(HitResult.Great, 3 * timing_unit),
        SimpleDifficultyRange(HitResult.Perfect, 1 * timing_unit),
    };

    private static readonly DifficultyRange[] gachi_ranges =
    {
        SimpleDifficultyRange(HitResult.Miss, 9 * timing_unit),
        SimpleDifficultyRange(HitResult.Ok, 9 * timing_unit),
        SimpleDifficultyRange(HitResult.Good, 6 * timing_unit),
        SimpleDifficultyRange(HitResult.Great, 3 * timing_unit),
        SimpleDifficultyRange(HitResult.Perfect, 1 * timing_unit),
    };

    protected override DifficultyRange[] GetDefaultRanges() => default_ranges;
    protected override DifficultyRange[] GetMajiRanges() => maji_ranges;
    protected override DifficultyRange[] GetGachiRanges() => gachi_ranges;
}
