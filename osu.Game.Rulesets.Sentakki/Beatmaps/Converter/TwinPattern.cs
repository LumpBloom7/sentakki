using System;
using System.Collections.Generic;
using Markdig.Extensions.Yaml;
using osu.Framework.Extensions.EnumExtensions;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

public class TwinPattern
{
    private static readonly TwinFlags[] allowedFlags = new TwinFlags[]{
        TwinFlags.None,
        TwinFlags.SpinCW,
        TwinFlags.SpinCCW,
        TwinFlags.Cycle,
        TwinFlags.Copy,
        TwinFlags.Mirror,
        TwinFlags.Copy | TwinFlags.Mirror
    };

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
        flags = allowedFlags[rng.Next(0, allowedFlags.Length)];
        originLane = rng.Next(0, 8);

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
        else if (flags.HasFlagFast(TwinFlags.Copy))
        {
            copyOffset = rng.Next(1, 7);
        }
    }

    public int getNextLane(int currentLane)
    {
        if (flags.HasFlagFast(TwinFlags.Cycle))
        {
            int tmp = originLane + cycleLanes[cycleIndex];
            cycleIndex = (cycleIndex + 1) % cycleLanes.Count;

            return tmp;
        }

        if (flags.HasFlagFast(TwinFlags.SpinCW))
            return originLane + (++spinIncrement);

        if (flags.HasFlagFast(TwinFlags.SpinCCW))
            return originLane + (--spinIncrement);


        int result = currentLane;
        if (flags.HasFlagFast(TwinFlags.Copy))
            result += copyOffset;

        if (flags.HasFlagFast(TwinFlags.Mirror))
            result = 7 - result;

        return result.NormalizePath();
    }
}
