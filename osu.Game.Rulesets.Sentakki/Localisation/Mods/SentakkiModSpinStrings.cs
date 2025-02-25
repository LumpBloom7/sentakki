using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation.Mods
{
    public static class SentakkiModSpinStrings
    {
        private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.Mods.SentakkiModSpinStrings";

        public static LocalisableString ModDescription => new TranslatableString(getKey(@"mod_description"), @"Replicate the true washing machine experience.");

        /// <summary>
        /// "Revolution duration"
        /// </summary>
        public static LocalisableString RevolutionDuration => new TranslatableString(getKey(@"revolution_duration"), @"Revolution duration");

        public static LocalisableString RevolutionDurationDescription => new TranslatableString(getKey(@"revolution_duration_description"), @"The duration in seconds to complete a revolution.");

        public static LocalisableString RevolutionDurationTooltip(int seconds)
            => new TranslatableString(getKey(@"entry_speed_tooltip"), @"{0}s", seconds);

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
