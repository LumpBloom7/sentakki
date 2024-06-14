using System;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModRelax : Mod, IApplicableAfterBeatmapConversion
    {
        public override Type[] IncompatibleMods => new[]
        {
            typeof(ModAutoplay),
        };

        public override string Name => "Relax";

        public override string Acronym => "RX";
        public override ModType Type => ModType.DifficultyReduction;

        public override LocalisableString Description => "All notes are EX notes, you've got nothing to prove!";

        public override double ScoreMultiplier => 0.3;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            foreach (SentakkiHitObject ho in beatmap.HitObjects)
                if (ho is IExNote exNote)
                    exNote.Ex = true;
        }
    }
}
