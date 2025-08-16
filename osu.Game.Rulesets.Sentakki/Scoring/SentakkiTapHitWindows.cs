using System;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring;

public class SentakkiTapHitWindows : SentakkiHitWindows
{
    protected override double DefaultWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Ok => 9 * TIMING_UNIT,
        HitResult.Good => 6 * TIMING_UNIT,
        HitResult.Great => 3 * TIMING_UNIT,
        HitResult.Perfect => 1 * TIMING_UNIT,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };

    protected override double MajiWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Ok => 6 * TIMING_UNIT,
        HitResult.Good => 3 * TIMING_UNIT,
        HitResult.Great => 2 * TIMING_UNIT,
        HitResult.Perfect => 1 * TIMING_UNIT,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };

    protected override double GachiWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Ok => 6 * TIMING_UNIT,
        HitResult.Good => 3 * TIMING_UNIT,
        HitResult.Great => 1 * TIMING_UNIT,
        HitResult.Perfect => 1 * TIMING_UNIT,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };
}
