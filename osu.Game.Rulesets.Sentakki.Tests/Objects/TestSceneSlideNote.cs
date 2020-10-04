using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    public class TestSceneSlideNote : TestSceneSentakkiHitObject
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
            beatmap.HitObjects.Add(new Slide
            {
                SlideInfoList = new List<SentakkiSlideInfo>
                {
                    new SentakkiSlideInfo {
                        ID = 25,
                        Duration = 1000,
                    },
                    new SentakkiSlideInfo {
                        ID = 27,
                        Duration = 1500,
                    },
                    new SentakkiSlideInfo {
                        ID = 0,
                        Duration = 2000,
                    }
                },
                StartTime = 500,
            });
            return beatmap;
        }

    }
}
