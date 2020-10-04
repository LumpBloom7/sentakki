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
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    public abstract class TestSceneHitObject : PlayerTestScene
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
