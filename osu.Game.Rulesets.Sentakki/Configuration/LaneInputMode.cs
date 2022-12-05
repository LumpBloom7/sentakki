using osu.Framework.Localisation;
using osu.Game.Rulesets.Sentakki.Localisation;

namespace osu.Game.Rulesets.Sentakki.Configuration
{
    public enum LaneInputMode
    {
        [LocalisableDescription(typeof(SentakkiSettingsSubsectionStrings), nameof(SentakkiSettingsSubsectionStrings.LaneInputModeButton))]
        Button,

        [LocalisableDescription(typeof(SentakkiSettingsSubsectionStrings), nameof(SentakkiSettingsSubsectionStrings.LaneInputModeSensor))]
        Sensor,
    }
}
