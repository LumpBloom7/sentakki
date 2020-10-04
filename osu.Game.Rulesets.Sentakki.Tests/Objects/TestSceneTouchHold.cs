using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    public class TestSceneTouchHold : TestSceneHitObject
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
            beatmap.HitObjects.Add(new TouchHold
            {
                StartTime = 500,
                Duration = 1500
            });

            return beatmap;
        }
    }
}
