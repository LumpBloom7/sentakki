using System.Collections.Generic;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Ranking.Statistics;

namespace osu.Game.Rulesets.Sentakki.Statistics;

public partial class MaimaiDxAccuracy : SimpleStatisticItem<double>
{
    public MaimaiDxAccuracy(IEnumerable<HitEvent> hitEvents)
        : base("MaimaiDX accuracy (approximated)")
    {
        Value = calculateDxAcc(hitEvents);
    }

    private static double calculateDxAcc(IEnumerable<HitEvent> hitEvents)
    {
        double maximum = 0;
        double actual = 0;

        int maxBonus = 0;
        float actualBonuses = 0;

        foreach (HitEvent hitEvent in hitEvents)
        {
            if (hitEvent.Result is HitResult.IgnoreMiss or HitResult.IgnoreHit)
                continue;

            maximum += ratioForHit(hitEvent.HitObject.Judgement.MaxResult);
            actual += ratioForHit(hitEvent.Result);

            if (hitEvent.HitObject is not SentakkiLanedHitObject sho) continue;

            if (!sho.Break) continue;

            maxBonus += 1 * sho.ScoreWeighting;
            if (hitEvent.Result == HitResult.Perfect)
                actualBonuses += 1 * sho.ScoreWeighting;
        }

        // If there are no regular notes, then a perfect play is vacuously true
        if (maximum == 0)
            actual = maximum = 1;

        // If there are no break notes, then the player vacuously hit all the breaks perfectly
        if (maxBonus == 0)
            actualBonuses = maxBonus = 1;

        return actual / maximum * 100 + actualBonuses / maxBonus;
    }

    private static float ratioForHit(HitResult result) => result switch
    {
        HitResult.Perfect => 1,
        HitResult.Great => 1,
        HitResult.Good => 0.8f,
        HitResult.Ok => 0.5f,
        _ => 0
    };

    protected override string DisplayValue(double value) => $"{value:N4}%";
}
