using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation.Mods
{
    public static class SentakkiModUnexceptionalStrings
    {
        private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.Mods.SentakkiModUnexceptionalStrings";

        public static LocalisableString ModDescription => new TranslatableString(getKey(@"mod_description"), @"Removes the EX modifier from all hitobjects.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
