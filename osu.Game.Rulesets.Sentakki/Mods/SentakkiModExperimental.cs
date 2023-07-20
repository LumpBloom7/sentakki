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

        public override bool UserPlayable => false;
        public override bool RequiresConfiguration => true;

        public override double ScoreMultiplier => 1.00;

        [SettingSource(typeof(SentakkiModExperimentalStrings), nameof(SentakkiModExperimentalStrings.TwinNotes), nameof(SentakkiModExperimentalStrings.TwinNotesDescription))]
        public BindableBool EnableTwinNotes { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };

        [SettingSource(typeof(SentakkiModExperimentalStrings), nameof(SentakkiModExperimentalStrings.TwinSlides), nameof(SentakkiModExperimentalStrings.TwinSlidesDescription))]
        public BindableBool EnableTwinSlides { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };

        [SettingSource(typeof(SentakkiModExperimentalStrings), nameof(SentakkiModExperimentalStrings.FanSlides), nameof(SentakkiModExperimentalStrings.FanSlidesDescription))]
        public BindableBool EnableSlideFans { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };

        [SettingSource("Revamped conversion",
            "A rewritten conversion system that hopefully makes better feeling conversions based on how notes are placed in the original beatmap. (Does not support twins)")]
        public BindableBool RevampedConversion { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };

        [SettingSource("Restore HitWhistle Slides", "Restores old Slide note conversion behavior where slides are only generated from sliders with hitWhistle")]
        public BindableBool HitWhistleSlides { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            var sentakkiBeatmapConverter = (SentakkiBeatmapConverter)beatmapConverter;
            if (EnableTwinNotes.Value)
                sentakkiBeatmapConverter.EnabledExperiments |= ConversionExperiments.twinNotes;

            if (EnableTwinSlides.Value)
                sentakkiBeatmapConverter.EnabledExperiments |= ConversionExperiments.twinSlides;

            if (EnableSlideFans.Value)
                sentakkiBeatmapConverter.EnabledExperiments |= ConversionExperiments.fanSlides;

            if (RevampedConversion.Value)
                sentakkiBeatmapConverter.EnabledExperiments |= ConversionExperiments.conversionRevamp;

            if (HitWhistleSlides.Value)
                sentakkiBeatmapConverter.EnabledExperiments |= ConversionExperiments.restoreSlideHitWhistle;
        }
    }
}
