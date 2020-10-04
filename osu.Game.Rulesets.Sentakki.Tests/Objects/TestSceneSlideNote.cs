using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Tests.Visual;
using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    public class TestSceneSlideNote : TestSceneHitObject
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
