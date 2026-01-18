using System;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring;

public class SentakkiSlideHitWindows : SentakkiHitWindows
{
    protected override double DefaultWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Meh => 36 * TIMING_UNIT,
        HitResult.Good => 26 * TIMING_UNIT,
        HitResult.Great => 14 * TIMING_UNIT,
        HitResult.Perfect => 14 * TIMING_UNIT,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };

    protected override double MajiWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Meh => 26 * TIMING_UNIT,
        HitResult.Good => 26 * TIMING_UNIT,
        HitResult.Great => 14 * TIMING_UNIT,
        HitResult.Perfect => 14 * TIMING_UNIT,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };
}
