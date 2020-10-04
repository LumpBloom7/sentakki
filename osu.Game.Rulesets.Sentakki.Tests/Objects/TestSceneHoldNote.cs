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

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    [TestFixture]
    public class TestSceneHoldNote : OsuTestScene
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private int depthIndex;

        public TestSceneHoldNote()
        {
            base.Content.Add(content = new SentakkiInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Miss Insane Short", () => testSingle(100));
            AddStep("Hit Insane Short", () => testSingle(100, true));
            AddStep("Miss Very Short", () => testSingle(200));
            AddStep("Hit Very Short", () => testSingle(200, true));
            AddStep("Miss Short", () => testSingle(500));
            AddStep("Hit Short", () => testSingle(500, true));
            AddStep("Miss Medium", () => testSingle(750));
            AddStep("Hit Medium", () => testSingle(750, true));
            AddStep("Miss Long", () => testSingle(1000));
            AddStep("Hit Long", () => testSingle(1000, true));
            AddStep("Miss Very Long", () => testSingle(3000));
            AddStep("Hit Very Long", () => testSingle(3000, true));
            AddUntilStep("Wait for object despawn", () => !Children.Any(h => (h is DrawableSentakkiHitObject) && (h as DrawableSentakkiHitObject).AllJudged == false));
        }

        private void testSingle(double duration, bool auto = false)
        {
            var circle = new Hold
            {
                StartTime = Time.Current + 1000,
                EndTime = Time.Current + 1000 + duration,
            };

            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { });

            Add(new TestDrawableHold(circle, auto)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Depth = depthIndex++,
            });
        }

        protected class TestDrawableHold : DrawableHold
        {
            private readonly bool auto;

            public TestDrawableHold(Hold h, bool auto)
                : base(h)
            {
                this.auto = auto;
            }

            public void TriggerJudgement() => UpdateResult(true);

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if (auto && !userTriggered && timeOffset > 0)
                {
                    // force success
                    ApplyResult(r => r.Type = r.Judgement.MaxResult);
                }
                else
                    base.CheckForResult(userTriggered, timeOffset);
            }
        }
    }
}
