using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Sentakki.Difficulty
{
    public partial class SentakkiDifficultyCalculator : DifficultyCalculator
    {
        public SentakkiDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        [GeneratedRegex("[+?]")]
        public static partial Regex PlusRegex();

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            double? starRating = null;
            if (beatmap.BeatmapInfo.Ruleset.ShortName == "Sentakki")
            {
                string[] tags = beatmap.Metadata.Tags.Split(' ');
                foreach (string tag in tags)
                {
                    if (string.IsNullOrWhiteSpace(tag))
                        continue;

                    bool isPlus = tag[^1] == '+';

                    string diffText = PlusRegex().Replace(tag, "");

                    if (float.TryParse(diffText.Trim(), out float diffNumber) && diffNumber > 0)
                    {
                        starRating = (diffNumber + (isPlus ? 0.5 : 0)) / 15.0 * 7.7;
                        break;
                    }
                }
                starRating ??= 10;
            }
            else
            {
                starRating = Math.Abs(beatmap.BeatmapInfo.StarRating * 1.25);
            }

            return new DifficultyAttributes
            {
                StarRating = starRating.Value,
                Mods = mods
            };
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate) => Array.Empty<DifficultyHitObject>();

        protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods, double clockRate) => Array.Empty<Skill>();
    }
}
