using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation;

public static class SentakkiBeatmapStrings
{
    private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.SentakkiBeatmapStrings";

    /// <summary>
    /// "Tap count"
    /// </summary>
    public static LocalisableString TapCount => new TranslatableString(getKey(@"taps"), @"Taps");

    /// <summary>
    /// "Hold count"
    /// </summary>
    public static LocalisableString HoldCount => new TranslatableString(getKey(@"holds"), @"Holds");

    /// <summary>
    /// "TouchHold count"
    /// </summary>
    public static LocalisableString TouchHoldCount => new TranslatableString(getKey(@"touchholds"), @"TouchHolds");

    /// <summary>
    /// "Touch count"
    /// </summary>
    public static LocalisableString TouchCount => new TranslatableString(getKey(@"touches"), @"Touches");

    /// <summary>
    /// "Slide count"
    /// </summary>
    public static LocalisableString SlideCount => new TranslatableString(getKey(@"slides"), @"Slides");

    private static string getKey(string key) => $"{prefix}:{key}";
}
