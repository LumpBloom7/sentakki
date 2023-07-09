using System;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

[Flags]
public enum ConversionExperiments
{
    none = 0,
    twinNotes = 1,
    twinSlides = 2,
    fanSlides = 4,
    conversionRevamp = 8
}
