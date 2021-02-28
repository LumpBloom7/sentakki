using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModHardRock : ModHardRock
    {
        public override double ScoreMultiplier => 1.06;
        public override bool Ranked => true;

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            difficulty.OverallDifficulty = 10f;
        }
    }
}
