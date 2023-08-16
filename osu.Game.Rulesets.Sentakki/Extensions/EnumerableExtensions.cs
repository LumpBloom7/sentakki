using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable, Random rng)
        => enumerable.OrderBy(_ => rng.Next());

    public static T ProbabilityPick<T>(this IEnumerable<T> enumerable, Func<T, double> probabilitySourceSelector, Random rng)
    {
        var list = enumerable.OrderBy(probabilitySourceSelector).ToList();

        double max = probabilitySourceSelector(list.Last());

        double pSum = 0;

        foreach (var item in list)
            pSum += max - probabilitySourceSelector(item);

        double ran = rng.NextSingle() * pSum;

        double cumulative = 0;

        foreach (var item in list)
        {
            cumulative += max - probabilitySourceSelector(item);

            if (ran <= cumulative)
                return item;
        }

        return list.Last();
    }
}
