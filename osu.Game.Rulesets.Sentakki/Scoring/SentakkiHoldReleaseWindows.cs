using System;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring;

public class SentakkiHoldReleaseWindows : SentakkiHitWindows
{
    protected override double DefaultWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Meh => 12 * TIMING_UNIT,
        HitResult.Good => 9 * TIMING_UNIT,
        HitResult.Great => 6 * TIMING_UNIT,
        HitResult.Perfect => 3 * TIMING_UNIT,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };

    protected override double MajiWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Meh => 9 * TIMING_UNIT,
        HitResult.Good => 6 * TIMING_UNIT,
        HitResult.Great => 3 * TIMING_UNIT,
        HitResult.Perfect => 1 * TIMING_UNIT,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };

    protected override double GachiWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Meh => 9 * TIMING_UNIT,
        HitResult.Good => 6 * TIMING_UNIT,
        HitResult.Great => 3 * TIMING_UNIT,
        HitResult.Perfect => 1 * TIMING_UNIT,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };
}
