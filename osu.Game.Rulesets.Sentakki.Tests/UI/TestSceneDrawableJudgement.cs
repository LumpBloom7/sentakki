using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki.Tests.UI;

[TestFixture]
public partial class TestSceneDrawableSentakkiJudgement : SentakkiSkinnableTestScene
{
    private List<JudgementPooler<DrawableSentakkiJudgement>> judgementPools = new List<JudgementPooler<DrawableSentakkiJudgement>>();

    private static IEnumerable<HitResult> validResults => new SentakkiRuleset().GetValidHitResults();

    [TestCaseSource(nameof(validResults))]
    public void Test(HitResult result)
    {
        AddStep("Display judgement", () => testResult(result));
    }

    private void testResult(HitResult result)
    {
        int poolIndex = 0;

        SetContents(_ =>
        {
            JudgementPooler<DrawableSentakkiJudgement> pool;

            if (poolIndex >= judgementPools.Count)
                judgementPools.Add(pool = new JudgementPooler<DrawableSentakkiJudgement>(CreateRuleset().GetValidHitResults()));
            else
            {
                // We need to make sure neither the pool nor the judgement get disposed when new content is set, and they both share the same parent.
                pool = judgementPools[poolIndex];
                ((Container)pool.Parent!).Clear(false);
            }

            var container = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = pool,
            };

            // Must be scheduled so the pool is loaded before we try and retrieve from it.
            Schedule(() =>
            {
                container.Add(pool.Get(result, j => j.Apply(new JudgementResult(new HitObject
                {
                    StartTime = Time.Current
                }, new SentakkiJudgement())
                {
                    Type = result,
                }, null!)));
            });

            poolIndex++;
            return container;
        });
    }
}
