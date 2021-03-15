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
        }

        private void createHitObject()
        {
            var ho = new Tap
            {
                StartTime = Time.Current + 1000,
            };
            lineRenderer.AddHitObject(ho);
        }
    }
}
