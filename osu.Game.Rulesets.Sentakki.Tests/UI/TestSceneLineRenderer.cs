using NUnit.Framework;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Rulesets.Sentakki.UI.Components.HitObjectLine;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.UI
{
    [TestFixture]
    public class TestSceneLineRenderer : OsuTestScene
    {
        private HitObjectLineRenderer lineRenderer;

        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        public TestSceneLineRenderer()
        {
            Add(lineRenderer = new HitObjectLineRenderer());
            AddStep("Create HitObject", () => createHitObject());
            AddStep("Create twin HitObjects (0)", () => createTwinHitObjects(0));
            AddStep("Create twin HitObjects (1)", () => createTwinHitObjects(1));
            AddStep("Create twin HitObjects (2)", () => createTwinHitObjects(2));
            AddStep("Create twin HitObjects (3)", () => createTwinHitObjects(3));
            AddStep("Create twin HitObjects (4)", () => createTwinHitObjects(4));
            AddStep("Create twin HitObjects (5)", () => createTwinHitObjects(5));
            AddStep("Create twin HitObjects (6)", () => createTwinHitObjects(6));
            AddStep("Create twin HitObjects (7)", () => createTwinHitObjects(7));
        }

        private void createHitObject()
        {
            var ho = new Tap
            {
                StartTime = Time.Current + 1000,
            };
            lineRenderer.AddHitObject(ho);
        }
        private void createTwinHitObjects(int difference)
        {
            var ho = new Tap
            {
                StartTime = Time.Current + 1000,
            };
            lineRenderer.AddHitObject(ho);

            var ho2 = new Tap
            {
                StartTime = Time.Current + 1000,
                Lane = difference
            };
            lineRenderer.AddHitObject(ho2);
        }
    }
}
