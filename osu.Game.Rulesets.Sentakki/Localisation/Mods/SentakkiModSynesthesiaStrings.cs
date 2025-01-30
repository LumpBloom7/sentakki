using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation.Mods;

public static class SentakkiModSynesthesiaStrings
{
    private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.Mods.SentakkiModSynesthesiaStrings";

    public static LocalisableString IntervalColouring
        => new TranslatableString(getKey(@"interval_colouring"), @"Interval colouring");

    public static LocalisableString IntervalColouringDescription
        => new TranslatableString(getKey(@"interval_colouring_description"), @"Colour hitobjects based on distance to previous/next hitobject.");

    private static string getKey(string key) => $"{prefix}:{key}";
}
