using System;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring;

public class SentakkiTouchHitWindows : SentakkiHitWindows
{
    protected override double DefaultWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Ok => 18 * timing_unit,
        HitResult.Good => 15 * timing_unit,
        HitResult.Great => 12 * timing_unit,
        HitResult.Perfect => 9 * timing_unit,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };

    protected override double MajiWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Ok => 15 * timing_unit,
        HitResult.Good => 12 * timing_unit,
        HitResult.Great => 10.5 * timing_unit,
        HitResult.Perfect => 9 * timing_unit,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };

    protected override double GachiWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Ok => 15 * timing_unit,
        HitResult.Good => 12 * timing_unit,
        HitResult.Great => 10.5 * timing_unit,
        HitResult.Perfect => 9 * timing_unit,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };
}
