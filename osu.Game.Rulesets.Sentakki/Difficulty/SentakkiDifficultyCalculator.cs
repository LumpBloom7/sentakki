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
            double starRating;
            if (beatmap.BeatmapInfo.Ruleset.ShortName == "Sentakki")
            {
                string diffText = beatmap.Metadata.Tags.Split(' ')[1];
                bool isPlus = diffText[^1] == '+';

                if (int.TryParse(diffText.Replace('+', ' ').Trim(), out int diffNumber) && diffNumber > 0)
                {
                    starRating = (diffNumber + (isPlus ? 0.5 : 0)) / 15.5 * 7.699999809265137;
                }
                else
                {
                    starRating = 10;
                }
            }
            else
            {
                starRating = beatmap.BeatmapInfo.StarRating * 1.25;
            }

            return new DifficultyAttributes
            {
                StarRating = starRating,
                Mods = mods
            };
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate) => Array.Empty<DifficultyHitObject>();

        protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods, double clockRate) => Array.Empty<Skill>();
    }
}
