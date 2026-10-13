using System;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring;

public class SentakkiTouchHoldReleaseWindows : SentakkiHitWindows
{
    protected override double DefaultWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Meh => 21 * TIMING_UNIT,
        HitResult.Good => 18 * TIMING_UNIT,
        HitResult.Great => 15 * TIMING_UNIT,
        HitResult.Perfect => 12 * TIMING_UNIT,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };

    protected override double MajiWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Meh => 18 * TIMING_UNIT,
        HitResult.Good => 15 * TIMING_UNIT,
        HitResult.Great => 12 * TIMING_UNIT,
        HitResult.Perfect => 10.5 * TIMING_UNIT,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };

    protected override double GachiWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Meh => 18 * TIMING_UNIT,
        HitResult.Good => 15 * TIMING_UNIT,
        HitResult.Great => 10.5 * TIMING_UNIT,
        HitResult.Perfect => 10.5 * TIMING_UNIT,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };
}
