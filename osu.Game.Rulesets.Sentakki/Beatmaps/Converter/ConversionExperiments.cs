using System;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

[Flags]
public enum ConversionFlags
{
    none = 0,
    twinNotes = 1,
    twinSlides = 2,
    fanSlides = 4,
    oldConverter = 8,
    disableCompositeSlides = 16
}
