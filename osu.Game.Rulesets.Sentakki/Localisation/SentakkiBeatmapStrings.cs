using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation
{
    public static class SentakkiBeatmapStrings
    {
        private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.SentakkiBeatmapStrings";

        /// <summary>
        /// "Tap count"
        /// </summary>
        public static LocalisableString TapCount => new TranslatableString(getKey(@"tap_count"), @"Tap count");

        /// <summary>
        /// "Hold count"
        /// </summary>
        public static LocalisableString HoldCount => new TranslatableString(getKey(@"hold_count"), @"Hold count");

        /// <summary>
        /// "TouchHold count"
        /// </summary>
        public static LocalisableString TouchHoldCount => new TranslatableString(getKey(@"touchhold_count"), @"TouchHold count");

        /// <summary>
        /// "Touch count"
        /// </summary>

        public static LocalisableString TouchCount => new TranslatableString(getKey(@"touch_count"), @"Touch count");

        /// <summary>
        /// "Slide count"
        /// </summary>
        public static LocalisableString SlideCount => new TranslatableString(getKey(@"slide_count"), @"Slide count");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
