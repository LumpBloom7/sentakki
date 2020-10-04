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
using osu.Framework.Utils;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    public class TestSceneTouchNote : TestSceneHitObject
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
                beatmap.HitObjects.Add(new Touch
                {
                    StartTime = 500 + (100 * i),
                    Position = SentakkiExtensions.GetCircularPosition(RNG.NextSingle(250), RNG.NextSingle(360))
                });

            return beatmap;
        }
    }
}
