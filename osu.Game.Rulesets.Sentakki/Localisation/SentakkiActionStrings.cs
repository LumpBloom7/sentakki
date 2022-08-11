using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation
{
    public static class SentakkiActionStrings
    {
        private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.SentakkiActionStrings";

        /// <summary>
        /// "Button 1"
        /// </summary>
        public static LocalisableString Button1 => new TranslatableString(getKey(@"button_1"), @"Button 1");

        /// <summary>
        /// "Button 2"
        /// </summary>
        public static LocalisableString Button2 => new TranslatableString(getKey(@"button_2"), @"Button 2");

        /// <summary>
        /// "Key 1"
        /// </summary>
        public static LocalisableString Key1 => new TranslatableString(getKey(@"key_1"), @"Key 1");

        /// <summary>
        /// "Key 2"
        /// </summary>
        public static LocalisableString Key2 => new TranslatableString(getKey(@"key_2"), @"Key 2");

        /// <summary>
        /// "Key 3"
        /// </summary>
        public static LocalisableString Key3 => new TranslatableString(getKey(@"key_3"), @"Key 3");

        /// <summary>
        /// "Key 4"
        /// </summary>
        public static LocalisableString Key4 => new TranslatableString(getKey(@"key_4"), @"Key 4");

        /// <summary>
        /// "Key 5"
        /// </summary>
        public static LocalisableString Key5 => new TranslatableString(getKey(@"key_5"), @"Key 5");

        /// <summary>
        /// "Key 6"
        /// </summary>
        public static LocalisableString Key6 => new TranslatableString(getKey(@"key_6"), @"Key 6");

        /// <summary>
        /// "Key 7"
        /// </summary>
        public static LocalisableString Key7 => new TranslatableString(getKey(@"key_7"), @"Key 7");

        /// <summary>
        /// "Key 8"
        /// </summary>
        public static LocalisableString Key8 => new TranslatableString(getKey(@"key_8"), @"Key 8");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
