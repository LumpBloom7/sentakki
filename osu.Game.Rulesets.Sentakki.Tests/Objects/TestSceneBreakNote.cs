using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    public class TestSceneBreakNote : TestSceneSentakkiHitObject
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap<SentakkiHitObject>()
            {
                BeatmapInfo =
                {
                    Ruleset = CreateRuleset()?.RulesetInfo ?? ruleset
                },
            };
            for (int i = 0; i < 8; ++i)
            {
                beatmap.HitObjects.Add(new Tap
                {
                    IsBreak = true,
                    StartTime = 500,
                    Lane = i
                });
                beatmap.HitObjects.Add(new Hold
                {
                    IsBreak = true,
                    StartTime = 1000,
                    Duration = 300,
                    Lane = (i + 3).NormalizePath()
                });
                beatmap.HitObjects.Add(new Slide
                {
                    IsBreak = true,
                    SlideInfoList = new List<SentakkiSlideInfo>
                    {
                        new SentakkiSlideInfo {
                            ID = 1,
                            Duration = 500,
                        }
                    },
                    StartTime = 1500,
                    Lane = (i + 7).NormalizePath()
                });
            }

            return beatmap;
        }
    }
}
