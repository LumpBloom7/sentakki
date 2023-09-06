using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation.Mods
{
    public static class SentakkiModHardRockStrings
    {
        private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.Mods.SentakkiModHardRockStrings";

        public static LocalisableString JudgementMode => new TranslatableString(getKey(@"judgement_mode"), @"Judgement mode");
        public static LocalisableString JudgementModeDescription => new TranslatableString(getKey(@"judgement_mode_description"), @"Judgement modes determine how strict the hitwindows are during gameplay.");

        public static LocalisableString MinimumResult => new TranslatableString(getKey(@"minimum_result"), @"Minimum hit result");
        public static LocalisableString MinimumResultDescription => new TranslatableString(getKey(@"minimum_result_description"), @"The minimum HitResult that is accepted during gameplay. Anything below will be considered a miss.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
