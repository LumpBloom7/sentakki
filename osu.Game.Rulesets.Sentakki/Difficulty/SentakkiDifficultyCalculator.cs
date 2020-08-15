using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using System;
using System.Collections.Generic;


namespace osu.Game.Rulesets.Sentakki.Difficulty
{
    public class SentakkiDifficultyCalculator : DifficultyCalculator
    {
        public SentakkiDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap) : base(ruleset, beatmap) { }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            return new DifficultyAttributes
            {
                StarRating = beatmap.BeatmapInfo.StarDifficulty * 1.25f, // Inflate SR of converts, to encourage players to try lower diffs, without hurting their fragile ego.
                Mods = mods,
                Skills = skills
            };
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate) => Array.Empty<DifficultyHitObject>();

        protected override Skill[] CreateSkills(IBeatmap beatmap) => Array.Empty<Skill>();
    }
}