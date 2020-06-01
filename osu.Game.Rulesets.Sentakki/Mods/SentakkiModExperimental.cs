using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModExperimental : Mod, IApplicableToBeatmapConverter
    {
        public override string Name => "Experimental";
        public override string Description => "Some experimental features to be added to future sentakki builds. Autoplay recommended.";
        public override string Acronym => "Ex";

        public override IconUsage? Icon => FontAwesome.Solid.Flask;
        public override ModType Type => ModType.Fun;

        public override bool Ranked => false;

        public override double ScoreMultiplier => 1.00;

        [SettingSource("Enable twin notes", "Allow more than one note to share the same times")]
        public BindableBool EnableTwins { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };

        [SettingSource("Replace TAP notes with TOUCH notes", "Allow TOUCHs to replace taps for testing")]
        public BindableBool EnableTouch { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            if (EnableTwins.Value)
                (beatmapConverter as SentakkiBeatmapConverter).EnabledExperiments |= ConversionExperiments.twins;

            if (EnableTouch.Value)
                (beatmapConverter as SentakkiBeatmapConverter).EnabledExperiments |= ConversionExperiments.touch;
        }
    }
}
