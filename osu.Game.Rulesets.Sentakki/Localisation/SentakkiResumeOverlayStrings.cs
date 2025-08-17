using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation;

public static class SentakkiResumeOverlayStrings
{
    private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.SentakkiResumeOverlayStrings";

    /// <summary>
    /// "Get ready!"
    /// </summary>
    public static LocalisableString GetReady => new TranslatableString(getKey(@"get_ready"), @"Get ready!");

    /// <summary>
    /// "Let's go!"
    /// </summary>
    public static LocalisableString LetsGo => new TranslatableString(getKey(@"lets_go"), @"Let's go!");

    /// <summary>
    /// "Sentakki is made with the support of {0}"
    /// </summary>
    public static LocalisableString SentakkiSupportedBy(string supporter) => new TranslatableString(getKey(@"sentakki_supported_by"), @"Sentakki is made with the support of {0}", supporter);

    private static string getKey(string key) => $"{prefix}:{key}";
}
