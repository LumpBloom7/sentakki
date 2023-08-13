using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation.Mods
{
    public static class SentakkiModMirrorStrings
    {
        private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.Mods.SentakkiModMirrorStrings";

        public static LocalisableString ModDescription => new TranslatableString(getKey(@"mod_description"), @"Flip the playfield horizontally, vertically, or both!");

        /// <summary>
        /// "⇅ Mirror vertically"
        /// </summary>
        public static LocalisableString MirrorVertically => new TranslatableString(getKey(@"mirror_vertically"), @"⇅ Mirror vertically");

        public static LocalisableString MirrorVerticallyDescription => new TranslatableString(getKey(@"mirror_vertically_description"), @"Mirror entire playfield across the x-axis");

        /// <summary>
        /// "⇆ Mirror horizontally"
        /// </summary>
        public static LocalisableString MirrorHorizontally => new TranslatableString(getKey(@"mirror_horizontally"), @"⇆ Mirror horizontally");

        public static LocalisableString MirrorHorizontallyDescription => new TranslatableString(getKey(@"mirror_horizontally_description"), @"Mirror entire playfield across the y-axis");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
