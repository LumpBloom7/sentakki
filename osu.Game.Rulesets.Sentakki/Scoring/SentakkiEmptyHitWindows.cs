using System;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring;

// A special case of EmptyHitWindows which also considers Gori, used by TOUCHHOLD
public class SentakkiEmptyHitWindows : SentakkiHitWindows
{
    protected override double DefaultWindowFor(HitResult result) => result switch
    {
        HitResult.Miss or HitResult.Perfect => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };
}
