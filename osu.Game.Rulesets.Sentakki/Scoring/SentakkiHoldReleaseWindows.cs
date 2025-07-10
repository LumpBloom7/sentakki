using System;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring;

public class SentakkiHoldReleaseWindows : SentakkiHitWindows
{
    protected override double DefaultWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Ok => 12 * timing_unit,
        HitResult.Good => 9 * timing_unit,
        HitResult.Great => 6 * timing_unit,
        HitResult.Perfect => 3 * timing_unit,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };

    protected override double MajiWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Ok => 9 * timing_unit,
        HitResult.Good => 6 * timing_unit,
        HitResult.Great => 3 * timing_unit,
        HitResult.Perfect => 1 * timing_unit,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };

    protected override double GachiWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Ok => 9 * timing_unit,
        HitResult.Good => 6 * timing_unit,
        HitResult.Great => 3 * timing_unit,
        HitResult.Perfect => 1 * timing_unit,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };
}
