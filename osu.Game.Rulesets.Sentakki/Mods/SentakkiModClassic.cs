using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.Beatmaps;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModClassic : ModClassic, IApplicableToBeatmapConverter
    {
        public override string Description => "Remove gameplay elements introduced in maimaiDX, for the Finale purists.";

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            ((SentakkiBeatmapConverter)beatmapConverter).ClassicMode = true;
        }
    }
}
