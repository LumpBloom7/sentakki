using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Localisation.Mods;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModClassic : ModClassic, IApplicableToBeatmapConverter
    {
        public override LocalisableString Description => SentakkiModClassicStrings.ModDescription;

        public override bool HasImplementation => false;

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            ((SentakkiBeatmapConverterOld)beatmapConverter).ClassicMode = true;
        }
    }
}
