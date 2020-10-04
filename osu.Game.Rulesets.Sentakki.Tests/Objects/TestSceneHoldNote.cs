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

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    public class TestSceneHoldNote : TestSceneHitObject
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
                beatmap.HitObjects.Add(new Hold
                {
                    StartTime = 500 + (200 * i),
                    Duration = 100 + (200 * i),
                    Lane = i
                });
            }
            return beatmap;
        }
    }
}
