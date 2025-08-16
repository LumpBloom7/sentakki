using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Sentakki.Beatmaps;

public class SentakkiPatternGenerator
{
    public readonly Random RNG;

    private int currentPattern;
    private int offset;
    private int offset2;

    public SentakkiPatternGenerator(IBeatmap beatmap)
    {
        var difficulty = beatmap.BeatmapInfo.Difficulty;
        int seed = (int)MathF.Round(difficulty.DrainRate + difficulty.CircleSize) * 20 + (int)(difficulty.OverallDifficulty * 41.2) + (int)MathF.Round(difficulty.ApproachRate);
        RNG = new Random(seed);
    }

    public int GetNextLane(bool isTwin = false) => patternlist[currentPattern].Invoke(isTwin).NormalizePath();

    public void StartNextPattern()
    {
        currentPattern = RNG.Next(0, patternlist.Count); // Pick a pattern
        offset = RNG.Next(0, 8); // Give it a random offset for variety
        offset2 = RNG.Next(-2, 3); // Give it a random offset for variety
    }

    //The patterns will generate the note lane to be used based on the current offset
    // argument list is (offset, diff)
    private List<Func<bool, int>> patternlist =>
    [
        twin =>
        {
            if (twin) return offset + 4;
            else offset += offset2;

            return offset;
        },
        // Back and forth, works better with longer combos
        // Lane difference determined by offset2, but will make sure offset2 is never 0.

        twin =>
        {
            offset2 = offset2 == 0 ? 1 : offset2;
            offset += offset2 * (twin ? 2 : 1);
            offset2 = -offset2;

            return offset;
        }
    ];
}
