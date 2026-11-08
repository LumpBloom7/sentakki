using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation.Mods;

public static class SentakkiModRelaxStrings
{
    private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.Mods.SentakkiModRelaxStrings";

    public static LocalisableString ModDescription =>
        new TranslatableString(getKey(@"mod_description"), @"You don't need to click. Just hover over the notes at the right time.");

    private static string getKey(string key) => $"{prefix}:{key}";
}
