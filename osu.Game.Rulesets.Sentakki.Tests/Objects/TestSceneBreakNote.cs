using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;
using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    public class TestSceneBreakNote : TestSceneHitObject
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
