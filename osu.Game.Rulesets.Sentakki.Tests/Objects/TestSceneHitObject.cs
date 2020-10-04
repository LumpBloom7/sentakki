using NUnit.Framework;
using osu.Game.Tests.Visual;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    public abstract class TestSceneSentakkiHitObject : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new SentakkiRuleset();

        protected override bool HasCustomSteps => true;

        private bool auto = false;
        protected override bool Autoplay => auto;

        [Test]
        public void TestMisses()
        {
            AddStep("Turn off auto", () => auto = false);
            CreateTest(null);
            AddUntilStep("Wait until all hitobjects are judged", () => Player.DrawableRuleset.Playfield.AllHitObjects.All(h => h.AllJudged));
        }

        [Test]
        public void TestHits()
        {
            AddStep("Turn on auto", () => auto = true);
            CreateTest(null);
            AddUntilStep("Wait until all hitobjects are judged", () => Player.DrawableRuleset.Playfield.AllHitObjects.All(h => h.AllJudged));
        }
    }
}
