using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    public class TestSceneTouchHold : TestSceneSentakkiHitObject
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
