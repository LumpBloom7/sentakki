using System.Collections.Generic;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking.Statistics;

namespace osu.Game.Rulesets.Sentakki.Statistics;

public partial class MaimaiDXAccuracy : SimpleStatisticItem<double>
{
    public MaimaiDXAccuracy(IEnumerable<HitEvent> hitEvents) : base("MaimaiDX accuracy (approximated)")
    {
        Value = calculateDXAcc(hitEvents);
    }

    private static double calculateDXAcc(IEnumerable<HitEvent> hitEvents)
    {
        double maximum = 0;
        double actual = 0;

        int maxBonus = 0;
        int actualBonuses = 0;

        foreach (HitEvent hitEvent in hitEvents)
        {
            switch (hitEvent.HitObject.Judgement.MaxResult)
            {
                case HitResult.Great:
                    maximum += 1;
                    break;

                case HitResult.LargeBonus:
                    maxBonus += 1;
                    break;
            }

            switch (hitEvent.Result)
            {
                case HitResult.Great:
                    actual += 1;
                    break;

                case HitResult.Good:
                    actual += 0.8;
                    break;

                case HitResult.Ok:
                    actual += 0.5;
                    break;

                case HitResult.LargeBonus:
                    actualBonuses += 1;
                    break;
            }
        }

        // If there are no regular notes, then a perfect play is vacuosly true
        if (maximum == 0)
            actual = maximum = 1;

        // If there are no break notes, then the player vacuosly hit all the breaks perfectly
        if (maxBonus == 0)
            actualBonuses = maxBonus = 1;

        return ((actual / maximum) * 100) + (actualBonuses / (float)maxBonus);
    }

    protected override string DisplayValue(double value) => $"{value:N4}%";
}
