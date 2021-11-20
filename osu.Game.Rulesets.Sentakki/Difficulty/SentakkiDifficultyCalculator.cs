using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Difficulty
{
    public class SentakkiDifficultyCalculator : DifficultyCalculator
    {
        public SentakkiDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap) : base(ruleset, beatmap) { }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            int maxCombo = 0;
            foreach (SentakkiHitObject h in beatmap.HitObjects)
            {
                switch (h)
                {
                    case Slide slide:
                        maxCombo += 1 + slide.SlideInfoList.Count + (slide.Break ? 4 : 0);
                        break;
                    case Hold hold:
                        maxCombo += 2 + (hold.Break ? 8 : 0);
                        break;
                    case Tap tap:
                        maxCombo += 1 + (tap.Break ? 4 : 0);
                        break;
                    default:
                        ++maxCombo;
                        break;
                }
            }

            return new DifficultyAttributes
            {
                StarRating = beatmap.BeatmapInfo.StarRating * 1.25f, // Inflate SR of converts, to encourage players to try lower diffs, without hurting their fragile ego.
                Mods = mods,
                Skills = skills,
                MaxCombo = maxCombo
            };
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate) => Array.Empty<DifficultyHitObject>();

        protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods, double clockRate) => Array.Empty<Skill>();
    }
}
