using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Sentakki.Difficulty
{
    public class SentakkiDifficultyCalculator : DifficultyCalculator
    {
        public SentakkiDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            int maxCombo = beatmap.GetMaxCombo();
            double starRating;

            if (beatmap.BeatmapInfo.Ruleset.ShortName == "Sentakki")
            {
                string difficultyName = beatmap.BeatmapInfo.DifficultyName.ToLower();

                if (difficultyName.Contains("re:master"))
                {
                    starRating = 7.0;
                }
                else if (difficultyName.Contains("master"))
                {
                    starRating = 6.0;
                }
                else if (difficultyName.Contains("expert"))
                {
                    starRating = 4.5;
                }
                else if (difficultyName.Contains("advanced"))
                {
                    starRating = 3.0;
                }
                else if (difficultyName.Contains("basic"))
                {
                    starRating = 2.0;
                }
                else if (difficultyName.Contains("easy"))
                {
                    starRating = 1.0;
                }
                else
                {
                    starRating = 10.0; // Probably utage
                }
            }
            else
            {
                starRating = beatmap.BeatmapInfo.StarRating * 1.25f;
            }

            return new DifficultyAttributes
            {
                StarRating = starRating, // Inflate SR of converts, to encourage players to try lower diffs, without hurting their fragile ego.
                Mods = mods,
                MaxCombo = maxCombo
            };
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate) => Array.Empty<DifficultyHitObject>();

        protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods, double clockRate) => Array.Empty<Skill>();
    }
}
