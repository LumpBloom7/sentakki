using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring;

public class SentakkiTouchHitWindows : SentakkiHitWindows
{
    private static readonly DifficultyRange[] default_ranges =
    {
        SimpleDifficultyRange(HitResult.Miss, 18 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Good, 18 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Great, 15 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Perfect, 12 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Critical, 9 * timing_unit)
    };

    private static readonly DifficultyRange[] maji_ranges =
    {
        SimpleDifficultyRange(HitResult.Miss, 15 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Good, 15 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Great, 12 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Perfect, 10.5 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Critical, 9 * timing_unit)
    };

    private static readonly DifficultyRange[] gachi_ranges =
    {
        SimpleDifficultyRange(HitResult.Miss, 15 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Good, 15 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Great, 12 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Perfect, 9 * timing_unit),
        SimpleDifficultyRange(SentakkiHitResult.Critical, 9 * timing_unit)
    };

    protected override DifficultyRange[] GetDefaultRanges() => default_ranges;
    protected override DifficultyRange[] GetMajiRanges() => maji_ranges;
    protected override DifficultyRange[] GetGachiRanges() => gachi_ranges;
}
