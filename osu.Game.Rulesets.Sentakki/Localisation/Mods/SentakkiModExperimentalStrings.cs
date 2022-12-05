using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation.Mods
{
    public static class SentakkiModExperimentalStrings
    {
        private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.Mods.SentakkiModExperimentalStrings";

        public static LocalisableString ModDescription => new TranslatableString(getKey(@"mod_description"),
            @"Some experimental features to be added to future sentakki builds. Autoplay/No-Fail recommended. Replays unsupported.");

        /// <summary>
        /// "Twin Notes"
        /// </summary>
        public static LocalisableString TwinNotes => new TranslatableString(getKey(@"twin_notes"), @"Twin notes");

        public static LocalisableString TwinNotesDescription => new TranslatableString(getKey(@"twin_notes_description"), @"Allow more than one note to share the same times (Requires multitouch)");

        /// <summary>
        /// "Twin Slides"
        /// </summary>
        public static LocalisableString TwinSlides => new TranslatableString(getKey(@"twin_slides"), @"Twin slides");

        public static LocalisableString TwinSlidesDescription =>
            new TranslatableString(getKey(@"twin_slides_description"), @"Allow more than one Slide-body to share the same time and origin (Requires multitouch)");

        /// <summary>
        /// "Fan Slides"
        /// </summary>
        public static LocalisableString FanSlides => new TranslatableString(getKey(@"fan_slides"), @"Fan slides");

        public static LocalisableString FanSlidesDescription => new TranslatableString(getKey(@"fan_slides_description"), @"Allow fan slides to occasionally appear (Requires multitouch)");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
