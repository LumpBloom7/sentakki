using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation
{
    public static class SentakkiStatisticsStrings
    {
        private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.SentakkiStatisticsStrings";

        /// <summary>
        /// "Timing Distribution"
        /// </summary>
        public static LocalisableString TimingDistribution => new TranslatableString(getKey(@"timing_distribution"), @"Timing Distribution");

        /// <summary>
        /// "Judgement Chart"
        /// </summary>
        public static LocalisableString JudgementChart => new TranslatableString(getKey(@"judgement_chart"), @"Judgement Chart");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
