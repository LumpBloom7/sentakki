using System;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

[Flags]
public enum TwinFlags
{
    None = 0,
    Mirror = 1 << 1, // The lane is horizontally mirrored from the main note
    Cycle = 1 << 2, // Cycles between 1 or more different lanes, prechosen
    SpinCW = 1 << 3, // Increments lane by 1 clockwise
    SpinCCW = 1 << 4, // Decrements lane by 1 counterclockwise
    Copy = 1 << 5, // Simply copies the main note, but with an offset
}
