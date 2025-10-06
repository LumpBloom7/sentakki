using System;

namespace osu.Game.Rulesets.Sentakki.Beatmaps.Converter;

[Flags]
public enum ConversionFlags
{
    None = 0,
    TwinNotes = 1,
    TwinSlides = 2,
    FanSlides = 4,
    OldConverter = 8,
    DisableCompositeSlides = 16
}
