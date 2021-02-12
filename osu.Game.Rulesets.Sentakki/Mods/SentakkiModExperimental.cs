using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.Beatmaps;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModExperimental : Mod, IApplicableToBeatmapConverter
    {
        public override string Name => "Experimental";
        public override string Description => "Some experimental features to be added to future sentakki builds. Autoplay/No-Fail recommended. Replays unsupported.";
        public override string Acronym => "Ex";

        public override IconUsage? Icon => FontAwesome.Solid.Flask;
        public override ModType Type => ModType.Fun;

        public override bool Ranked => false;
        public override bool RequiresConfiguration => true;

        public override double ScoreMultiplier => 1.00;

        [SettingSource("Twin notes", "Allow more than one note to share the same times")]
        public BindableBool EnableTwinNotes { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };

        [SettingSource("Twin slides", "Allow more than one note to share the same times")]
        public BindableBool EnableTwinSlides { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };

        [SettingSource("Touch notes", "Allow TOUCHs to appear")]
        public BindableBool EnableTouch { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            if (EnableTwinNotes.Value)
                (beatmapConverter as SentakkiBeatmapConverter).EnabledExperiments.Value |= ConversionExperiments.twinNotes;

            if (EnableTwinSlides.Value)
                (beatmapConverter as SentakkiBeatmapConverter).EnabledExperiments.Value |= ConversionExperiments.twinSlides;

            if (EnableTouch.Value)
                (beatmapConverter as SentakkiBeatmapConverter).EnabledExperiments.Value |= ConversionExperiments.touch;
        }
    }
}
