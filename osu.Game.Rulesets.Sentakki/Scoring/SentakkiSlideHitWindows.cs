using System;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring;

public class SentakkiSlideHitWindows : SentakkiHitWindows
{
    protected override double DefaultWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Ok => 36 * timing_unit,
        HitResult.Good => 26 * timing_unit,
        HitResult.Great => 14 * timing_unit,
        HitResult.Perfect => 14 * timing_unit,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };

    protected override double MajiWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Ok => 26 * timing_unit,
        HitResult.Good => 26 * timing_unit,
        HitResult.Great => 14 * timing_unit,
        HitResult.Perfect => 14 * timing_unit,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };
}
