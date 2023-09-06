using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring;

// A special case of EmptyHitWindows which also considers Gori, used by HOLD and TOUCHHOLD
public class SentakkiEmptyHitWindows : SentakkiHitWindows
{
    private static readonly DifficultyRange[] empty_ranges =
    {
        SimpleDifficultyRange(HitResult.Perfect, 0),
        SimpleDifficultyRange(HitResult.Miss, 0),
    };

    public override bool IsHitResultAllowed(HitResult result)
    {
        // We additionally allow HitResult.Great because we don't acknowledge crits for those objects
        if (result is HitResult.Great or HitResult.Miss)
            return true;

        return base.IsHitResultAllowed(result);
    }

    protected override DifficultyRange[] GetDefaultRanges() => empty_ranges;
}
