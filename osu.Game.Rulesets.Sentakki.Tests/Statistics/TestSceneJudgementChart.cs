using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Statistics;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.Statistics
{
    [TestFixture]
    public partial class TestSceneJudgementChart : OsuTestScene
    {
        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        private List<HitEvent> testevents = new List<HitEvent>
        {
            // Tap
            new HitEvent(0, HitResult.Great, new Tap(), new Tap(), null),
            new HitEvent(0, HitResult.Great, new Tap(), new Tap(), null),
            new HitEvent(0, HitResult.Great, new Tap(), new Tap(), null),
            new HitEvent(0, HitResult.Great, new Tap(), new Tap(), null),
            new HitEvent(0, HitResult.Great, new Tap(), new Tap(), null),
            new HitEvent(0, HitResult.Great, new Tap(), new Tap(), null),
            new HitEvent(0, HitResult.Good, new Tap(), new Tap(), null),
            new HitEvent(0, HitResult.Good, new Tap(), new Tap(), null),
            new HitEvent(0, HitResult.Good, new Tap(), new Tap(), null),
            new HitEvent(0, HitResult.Good, new Tap(), new Tap(), null),
            new HitEvent(0, HitResult.Good, new Tap(), new Tap(), null),
            new HitEvent(0, HitResult.Ok, new Tap(), new Tap(), null),
            new HitEvent(0, HitResult.Ok, new Tap(), new Tap(), null),
            new HitEvent(0, HitResult.Ok, new Tap(), new Tap(), null),
            new HitEvent(0, HitResult.Miss, new Tap(), new Tap(), null),
            new HitEvent(0, HitResult.Miss, new Tap(), new Tap(), null),
            // Holds
            new HitEvent(0, HitResult.Great, new Hold(), new Tap(), null),
            new HitEvent(0, HitResult.Great, new Hold(), new Tap(), null),
            // Touch
            new HitEvent(0, HitResult.Good, new Touch(), new Tap(), null),
            new HitEvent(0, HitResult.Good, new Touch(), new Tap(), null),
            new HitEvent(0, HitResult.Good, new Touch(), new Tap(), null),
            new HitEvent(0, HitResult.Good, new Touch(), new Tap(), null),
            new HitEvent(0, HitResult.Ok, new Touch(), new Tap(), null),
            new HitEvent(0, HitResult.Ok, new Touch(), new Tap(), null),
            new HitEvent(0, HitResult.Ok, new Touch(), new Tap(), null),
            new HitEvent(0, HitResult.Miss, new Touch(), new Tap(), null),
            new HitEvent(0, HitResult.Miss, new Touch(), new Tap(), null),
            new HitEvent(0, HitResult.Great, new Touch(), new Tap(), null),
            new HitEvent(0, HitResult.Great, new Touch(), new Tap(), null),
            // Breaks
            new HitEvent(0, HitResult.Great, new Tap { Break = true }, new Tap(), null),
            new HitEvent(0, HitResult.Good, new Tap { Break = true }, new Tap(), null),
            new HitEvent(0, HitResult.Good, new Tap { Break = true }, new Tap(), null),
            new HitEvent(0, HitResult.Good, new Tap { Break = true }, new Tap(), null),
            new HitEvent(0, HitResult.Good, new Tap { Break = true }, new Tap(), null),
            new HitEvent(0, HitResult.Good, new Tap { Break = true }, new Tap(), null),
            new HitEvent(0, HitResult.Ok, new Tap { Break = true }, new Tap(), null),
        };

        public TestSceneJudgementChart()
        {
            Add(new JudgementChart(testevents));
        }
    }
}
