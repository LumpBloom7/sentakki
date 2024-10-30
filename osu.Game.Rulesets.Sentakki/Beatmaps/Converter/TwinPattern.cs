using System;
using System.Collections.Generic;
using Markdig.Extensions.Yaml;
using osu.Framework.Extensions.EnumExtensions;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

public class TwinPattern
{
    private TwinFlags flags;

    private List<int> cycleLanes = new List<int>();
    private int cycleIndex = 0;

    private int originLane = 0;

    private int spinIncrement = 0;

    private int copyOffset = 1;

    private Random rng;

    public TwinPattern(Random rng)
    {
        this.rng = rng;
        NewPattern();
    }

    public void NewPattern()
    {
        flags = (TwinFlags)(1 << (rng.Next(1, 6)));
        originLane = rng.Next(0, 4);

        if (flags.HasFlagFast(TwinFlags.Cycle))
        {
            cycleLanes.Clear();
            cycleLanes.Add(rng.Next(0, 8));
            cycleIndex = 0;

            float prob = 0.75f;

            while (true)
            {
                if (rng.NextSingle() > prob)
                    break;

                cycleLanes.Add(rng.Next(0, 8));
                prob *= 0.5f;
            }
        }
    }

    public int getNextLane(int currentLane)
    {
        if (flags.HasFlagFast(TwinFlags.Mirror))
            return 7 - currentLane;

        if (flags.HasFlagFast(TwinFlags.SpinCW))
            return originLane + (++spinIncrement);

        if (flags.HasFlagFast(TwinFlags.SpinCCW))
            return originLane + (--spinIncrement);

        if (flags.HasFlagFast(TwinFlags.Cycle))
        {
            int tmp = originLane + cycleLanes[cycleIndex];
            cycleIndex = (cycleIndex + 1) % cycleLanes.Count;

            return tmp;
        }

        if (flags.HasFlagFast(TwinFlags.Copy))
        {
            return currentLane + copyOffset;
        }
        return currentLane;
    }


}
