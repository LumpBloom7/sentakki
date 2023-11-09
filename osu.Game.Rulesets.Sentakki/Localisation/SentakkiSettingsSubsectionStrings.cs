using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation
{
    public static class SentakkiSettingsSubsectionStrings
    {
        private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.SentakkiSettingsSubsectionStrings";

        /// <summary>
        /// "Show kiai effects"
        /// </summary>
        public static LocalisableString ShowKiaiEffects => new TranslatableString(getKey(@"show_kiai_effects"), @"Show Kiai effects");

        /// <summary>
        /// "Show note start indicators"
        /// </summary>
        public static LocalisableString ShowNoteStartIndicators => new TranslatableString(getKey(@"show_note_start_indicators"), @"Show note start indicators");

        /// <summary>
        /// "Snaking in Slides"
        /// </summary>
        public static LocalisableString SnakingInSlides => new TranslatableString(getKey(@"snaking_in_slides"), @"Snaking in Slides");

        /// <summary>
        /// "ShowDetailedJudgements"
        /// </summary>
        public static LocalisableString ShowDetailedJudgements => new TranslatableString(getKey(@"show_detailed_judgements"), @"Show detailed judgements");

        /// <summary>
        /// "Ring colour"
        /// </summary>
        public static LocalisableString RingColor => new TranslatableString(getKey(@"ring_color"), @"Ring colour");

        public static LocalisableString RingColorDefault => new TranslatableString(getKey(@"ring_color_default"), @"Default");
        public static LocalisableString RingColorDifficulty => new TranslatableString(getKey(@"ring_color_difficulty"), @"Difficulty-based colour");
        public static LocalisableString RingColorSkin => new TranslatableString(getKey(@"ring_color_skin"), @"Skin");

        /// <summary>
        /// "Note entry speed"
        /// </summary>
        public static LocalisableString NoteEntrySpeed => new TranslatableString(getKey(@"note_entry_speed"), @"Note entry speed");

        /// <summary>
        /// "Touch note fade-in speed"
        /// </summary>
        public static LocalisableString TouchNoteFadeInSpeed => new TranslatableString(getKey(@"touch_note_fade_in_speed"), @"Touch note fade-in speed");

        /// <summary>
        /// "Ring Opacity"
        /// </summary>
        public static LocalisableString RingOpacity => new TranslatableString(getKey(@"ring_opacity"), @"Ring Opacity");

        /// <summary>
        /// "Lane input mode (Doesn't apply to touch)"
        /// </summary>
        public static LocalisableString LaneInputMode => new TranslatableString(getKey(@"lane_input_mode"), @"Lane input mode (Doesn't apply to touch)");

        public static LocalisableString LaneInputModeButton => new TranslatableString(getKey(@"lane_input_mode_button"), @"Button");
        public static LocalisableString LaneInputModeSensor => new TranslatableString(getKey(@"lane_input_mode_sensor"), @"Sensor");

        /// <summary>
        /// "Break sample volume"
        /// </summary>
        public static LocalisableString BreakSampleVolume => new TranslatableString(getKey(@"break_sample_volume"), @"Break sample volume");

        public static LocalisableString EntrySpeedTooltip(float speed, double time)
            => new TranslatableString(getKey(@"entry_speed_tooltip"), @"{0} ({1}ms)", speed > 10 ? Sonic : $"{speed:N1}", $"{time:N0}");

        public static LocalisableString Sonic => new TranslatableString(getKey(@"sonic"), @"Sonic");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
