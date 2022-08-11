using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation
{
    public static class SentakkiActionStrings
    {
        private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.SentakkiActionStrings";

        /// <summary>
        /// "Button #"
        /// </summary>
        private static LocalisableString button(int number) => new TranslatableString(getKey(@"button_#"), @"Button {0}", number);
        public static LocalisableString Button1 => button(1);
        public static LocalisableString Button2 => button(2);

        /// <summary>
        /// "Key #"
        /// </summary>
        private static LocalisableString key(int number) => new TranslatableString(getKey(@"key_#"), @"Key {0}", number);
        public static LocalisableString Key1 => key(1);
        public static LocalisableString Key2 => key(2);
        public static LocalisableString Key3 => key(3);
        public static LocalisableString Key4 => key(4);
        public static LocalisableString Key5 => key(5);
        public static LocalisableString Key6 => key(6);
        public static LocalisableString Key7 => key(7);
        public static LocalisableString Key8 => key(8);

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
