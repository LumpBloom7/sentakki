using osu.Framework.Localisation;
using osu.Game.Rulesets.Sentakki.Localisation;

namespace osu.Game.Rulesets.Sentakki.Configuration
{
    public enum ColorOption
    {
        [LocalisableDescription(typeof(SentakkiSettingsSubsectionStrings), nameof(SentakkiSettingsSubsectionStrings.RingColorDefault))]
        Default,

        [LocalisableDescription(typeof(SentakkiSettingsSubsectionStrings), nameof(SentakkiSettingsSubsectionStrings.RingColorDifficulty))]
        Difficulty,

        [LocalisableDescription(typeof(SentakkiSettingsSubsectionStrings), nameof(SentakkiSettingsSubsectionStrings.RingColorSkin))]
        Skin,
    }
}
