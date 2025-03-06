using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation.Mods
{
    public static class SentakkiModDifficultyAdjustStrings
    {
        private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.Mods.SentakkiModDifficultyAdjustStrings";

        public static LocalisableString BreakRemoval => new TranslatableString(getKey(@"break_removal"), @"No BREAK notes");
        public static LocalisableString ExRemoval => new TranslatableString(getKey(@"ex_removal"), @"No EX notes");
        public static LocalisableString AllEx => new TranslatableString(getKey(@"all_ex"), @"All EX notes");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
