using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation.Mods
{
    public static class SentakkiModAutoTouchStrings
    {
        private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.Mods.SentakkiModAutoTouchStrings";

        public static LocalisableString ModDescription => new TranslatableString(getKey(@"mod_description"), @"Focus on the laned notes. Touch notes and Slide bodies will be automatically completed.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
