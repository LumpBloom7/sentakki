using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation.Mods
{
    public static class SentakkiModChallengeStrings
    {
        private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.Mods.SentakkiModChallengeStrings";

        public static LocalisableString ModDescription => new TranslatableString(getKey(@"mod_description"), @"You only get a small margin for errors.");

        /// <summary>
        /// "Number of Lives"
        /// </summary>
        public static LocalisableString NumberOfLives => new TranslatableString(getKey(@"number_of_lives"), @"Number of lives");
        public static LocalisableString NumberOfLivesDescription => new TranslatableString(getKey(@"number_of_lives_description"), @"The number of lives you start with.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
