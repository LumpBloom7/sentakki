using System;
using System.Collections.Generic;
using osu.Framework.Extensions.EnumExtensions;
using osu.Game.Rulesets.Sentakki.Extensions;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

public class TwinPattern
{
    private static readonly TwinFlags[] allowed_flags =
    [
        TwinFlags.None,
        TwinFlags.SpinCw,
        TwinFlags.SpinCcw,
        TwinFlags.Cycle,
        TwinFlags.Copy,
        TwinFlags.Mirror,
        TwinFlags.Copy | TwinFlags.Mirror
    ];

    private TwinFlags flags;

    private readonly List<int> cycleLanes = [];
    private int cycleIndex;

    private int originLane;

    private int spinIncrement;

    private int copyOffset = 1;

    private readonly Random rng;

    public TwinPattern(Random rng)
    {
        this.rng = rng;

        NewPattern();
    }

    public void NewPattern()
    {
        flags = allowed_flags[rng.Next(0, allowed_flags.Length)];
        originLane = rng.Next(0, 8);

        if (flags.HasFlagFast(TwinFlags.Cycle))
        {
            cycleLanes.Clear();
            cycleLanes.Add(rng.Next(0, 8));
            cycleIndex = 0;

            float prob = 0.75f;

            while (true)
            {
                if (rng.NextDouble() > prob)
                    break;

                cycleLanes.Add(rng.Next(0, 8));
                prob *= 0.5f;
            }
        }
        else if (flags.HasFlagFast(TwinFlags.Copy))
        {
            copyOffset = rng.Next(1, 7);
        }
    }

    public int GetNextLane(int currentLane)
    {
        if (flags.HasFlagFast(TwinFlags.Cycle))
        {
            int tmp = originLane + cycleLanes[cycleIndex];
            cycleIndex = (cycleIndex + 1) % cycleLanes.Count;

            return tmp;
        }

        if (flags.HasFlagFast(TwinFlags.SpinCw))
            return originLane + ++spinIncrement;

        if (flags.HasFlagFast(TwinFlags.SpinCcw))
            return originLane + --spinIncrement;

        int result = currentLane;
        if (flags.HasFlagFast(TwinFlags.Copy))
            result += copyOffset;

        if (flags.HasFlagFast(TwinFlags.Mirror))
            result = 7 - result;

        return result.NormalizeLane();
    }
}
