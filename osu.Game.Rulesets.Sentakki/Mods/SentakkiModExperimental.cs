using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Beatmaps.Converter;
using osu.Game.Rulesets.Sentakki.Localisation.Mods;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModExperimental : Mod, IApplicableToBeatmapConverter
    {
        public override string Name => "Experimental";
        public override LocalisableString Description => SentakkiModExperimentalStrings.ModDescription;
        public override string Acronym => "Ex";

        public override IconUsage? Icon => FontAwesome.Solid.Flask;
        public override ModType Type => ModType.Conversion;

        public override bool RequiresConfiguration => true;

        public override double ScoreMultiplier => 1.00;

        [SettingSource(typeof(SentakkiModExperimentalStrings), nameof(SentakkiModExperimentalStrings.FanSlides), nameof(SentakkiModExperimentalStrings.FanSlidesDescription))]
        public Bindable<bool> EnableSlideFans { get; } = new BindableBool(false);

        [SettingSource("Use old converter", "The old converter relied on RNG for just about everything. Included for comparison purposes.")]
        public BindableBool OldConversion { get; } = new BindableBool(false);

        [SettingSource(typeof(SentakkiModExperimentalStrings), nameof(SentakkiModExperimentalStrings.TwinNotes), nameof(SentakkiModExperimentalStrings.TwinNotesDescription))]
        public BindableBool EnableTwinNotes { get; } = new BindableBool(false);

        [SettingSource(typeof(SentakkiModExperimentalStrings), nameof(SentakkiModExperimentalStrings.TwinSlides), nameof(SentakkiModExperimentalStrings.TwinSlidesDescription))]
        public BindableBool EnableTwinSlides { get; } = new BindableBool(false);

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            var sentakkiBeatmapConverter = (CompositeBeatmapConverter)beatmapConverter;

            if (EnableSlideFans.Value)
                sentakkiBeatmapConverter.flags |= ConversionFlags.fanSlides;

            if (OldConversion.Value)
                sentakkiBeatmapConverter.flags |= ConversionFlags.oldConverter;

            if (EnableTwinNotes.Value)
                sentakkiBeatmapConverter.flags |= ConversionFlags.twinNotes;

            if (EnableTwinSlides.Value)
                sentakkiBeatmapConverter.flags |= ConversionFlags.twinSlides;
        }
    }
}
