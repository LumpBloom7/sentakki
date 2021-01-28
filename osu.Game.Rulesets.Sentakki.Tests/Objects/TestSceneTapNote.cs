using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    public class TestSceneTapNote : TestSceneSentakkiHitObject
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
