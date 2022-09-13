using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.Beatmaps;
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

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            if (EnableTwinNotes.Value)
                (beatmapConverter as SentakkiBeatmapConverter).EnabledExperiments |= ConversionExperiments.twinNotes;

            if (EnableTwinSlides.Value)
                (beatmapConverter as SentakkiBeatmapConverter).EnabledExperiments |= ConversionExperiments.twinSlides;

            if (EnableSlideFans.Value)
                (beatmapConverter as SentakkiBeatmapConverter).EnabledExperiments |= ConversionExperiments.fanSlides;
        }
    }
}
