using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation.Mods
{
    public static class SentakkiModAutoTouch
    {
        private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.Mods.SentakkiModClassicStrings";

        public static LocalisableString ModDescription => new TranslatableString(getKey(@"mod_description"), @"Remove gameplay elements introduced in maimaiDX, for the Finale purists.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
