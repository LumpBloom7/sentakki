using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring;

public class SentakkiTapHitWindows : SentakkiHitWindows
{
    private static readonly DifficultyRange[] default_ranges =
    {
        SimpleDifficultyRange(HitResult.Miss, 9 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Good, 9 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Great, 6 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Perfect, 3 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Critical, 1 * timing_unit),
    };

    private static readonly DifficultyRange[] maji_ranges =
    {
        SimpleDifficultyRange(HitResult.Miss, 6 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Good, 6 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Great, 3 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Perfect, 2 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Critical, 1 * timing_unit),
    };

    private static readonly DifficultyRange[] gachi_ranges =
    {
        SimpleDifficultyRange(HitResult.Miss, 6 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Good, 6 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Great, 3 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Perfect, 1 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Critical, 1 * timing_unit),
    };

    protected override DifficultyRange[] GetDefaultRanges() => default_ranges;
    protected override DifficultyRange[] GetMajiRanges() => maji_ranges;
    protected override DifficultyRange[] GetGachiRanges() => gachi_ranges;
}
