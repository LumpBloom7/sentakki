using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Beatmaps.Converter;
using osu.Game.Rulesets.Sentakki.Localisation.Mods;

namespace osu.Game.Rulesets.Sentakki.Mods;

public class SentakkiModExperimental : Mod, IApplicableToBeatmapConverter
{
    public override string Name => "Experimental";
    public override LocalisableString Description => SentakkiModExperimentalStrings.ModDescription;
    public override string Acronym => "Ex";

    public override IconUsage? Icon => FontAwesome.Solid.Flask;
    public override ModType Type => ModType.Conversion;

    public override bool RequiresConfiguration => true;

    public override double ScoreMultiplier => 1.00;

    public override bool Ranked => true;

    [SettingSource(typeof(SentakkiModExperimentalStrings), nameof(SentakkiModExperimentalStrings.FanSlides), nameof(SentakkiModExperimentalStrings.FanSlidesDescription))]
    public Bindable<bool> EnableSlideFans { get; } = new BindableBool();

    [SettingSource("Use old converter", "The old converter relied on RNG for just about everything. Included for comparison purposes.")]
    public BindableBool OldConversion { get; } = new BindableBool();

    [SettingSource(typeof(SentakkiModExperimentalStrings), nameof(SentakkiModExperimentalStrings.TwinNotes), nameof(SentakkiModExperimentalStrings.TwinNotesDescription))]
    public BindableBool EnableTwinNotes { get; } = new BindableBool();

    [SettingSource(typeof(SentakkiModExperimentalStrings), nameof(SentakkiModExperimentalStrings.TwinSlides), nameof(SentakkiModExperimentalStrings.TwinSlidesDescription))]
    public BindableBool EnableTwinSlides { get; } = new BindableBool();

    public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
    {
        if (beatmapConverter is not CompositeBeatmapConverter sentakkiBeatmapConverter)
            return;

        if (EnableSlideFans.Value)
            sentakkiBeatmapConverter.Flags |= ConversionFlags.FanSlides;

        if (OldConversion.Value)
            sentakkiBeatmapConverter.Flags |= ConversionFlags.OldConverter;

        if (EnableTwinNotes.Value)
            sentakkiBeatmapConverter.Flags |= ConversionFlags.TwinNotes;

        if (EnableTwinSlides.Value)
            sentakkiBeatmapConverter.Flags |= ConversionFlags.TwinSlides;
    }
}
