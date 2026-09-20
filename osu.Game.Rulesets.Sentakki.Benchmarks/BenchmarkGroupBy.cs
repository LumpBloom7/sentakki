using BenchmarkDotNet.Attributes;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Benchmarks;

[MemoryDiagnoser]
public partial class BenchmarkGroupBy
{
    private List<HitObject> hitObjects = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        hitObjects = new List<HitObject>();

        for (int i = 0; i < 50000; ++i)
        {
            int number = Random.Shared.Next(1, 5);

            double startTime = Random.Shared.NextDouble() * 180000;

            for (int j = 0; j < number; ++j)
            {
                hitObjects.Add(new Tap
                {
                    StartTime = startTime,
                    Break = Random.Shared.Next(0, 1) == 1
                });
            }
        }

        hitObjects = [.. hitObjects.OrderBy(h => h.StartTime)];
    }

    [Benchmark]
    public int GroupByLinq()
    {
        var groups = hitObjects.GroupBy(h => h.StartTime);

        int sum = 0;

        foreach (var group in groups)
        {
            foreach (var ho in group)
            {
                if (ho is Tap t && t.Break)
                    ++sum;
            }
        }

        foreach (var group in groups)
        {
            foreach (var ho in group)
            {
                if (ho is Tap t && Math.Round(t.StartTime) % 2 == 0)
                    ++sum;
            }
        }

        return sum;
    }

    [Benchmark]
    public int GroupByDictionary()
    {
        var groups = hitObjects.GroupByDictionary(h => h.StartTime);

        int sum = 0;

        foreach (var group in groups)
        {
            foreach (var ho in group.Value)
            {
                if (ho is Tap t && t.Break)
                    ++sum;
            }
        }

        foreach (var group in groups)
        {
            foreach (var ho in group.Value)
            {
                if (ho is Tap t && Math.Round(t.StartTime) % 2 == 0)
                    ++sum;
            }
        }

        return sum;
    }
}
