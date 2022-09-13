using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation.Mods
{
    public static class SentakkiModHiddenStrings
    {
        private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.Mods.SentakkiModHiddenStrings";

        public static LocalisableString ModDescription => new TranslatableString(getKey(@"mod_description"), @"Notes fade out just before you hit them.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
