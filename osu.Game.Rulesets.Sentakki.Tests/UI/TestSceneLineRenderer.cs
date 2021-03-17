using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Rulesets.Sentakki.UI.Components.HitObjectLine;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

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
            AddStep("Create HitObject", () => createHitObjects());
            AddStep("Create twins", () => createHitObjects(2));
            AddStep("Create triplets", () => createHitObjects(3));
            AddStep("Create quadruplets", () => createHitObjects(4));
        }

        private void createHitObjects(int amount = 1)
        {
            for (int i = 0; i < amount; ++i)
            {
                var lane = RNG.Next(0, 8);
                var ho = new Tap
                {
                    StartTime = Time.Current + 1000,
                    Lane = lane
                };
                lineRenderer.AddHitObject(ho);

                Add(new Circle
                {
                    Size = new Vector2(20),
                    Colour = Color4.White,
                    LifetimeEnd = Time.Current + 1000,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Position = SentakkiExtensions.GetPositionAlongLane(300, lane)
                });
            }
        }
    }
}
