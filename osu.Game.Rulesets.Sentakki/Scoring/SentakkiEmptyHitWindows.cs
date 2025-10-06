using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring;

// A special case of EmptyHitWindows which also considers Gori, used by TOUCHHOLD
public class SentakkiEmptyHitWindows : SentakkiHitWindows
{
    protected override double DefaultWindowFor(HitResult result) => 0;
}
