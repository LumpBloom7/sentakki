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
    public class TestSceneTapNote : TestSceneHitObject
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
                beatmap.HitObjects.Add(new Tap
                {
                    StartTime = 500 + (100 * i),
                    Lane = i
                });

            return beatmap;
        }
    }
}
