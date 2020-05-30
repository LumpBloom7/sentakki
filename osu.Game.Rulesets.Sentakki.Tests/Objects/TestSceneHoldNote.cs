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
        }

        private void testSingle(double duration, bool auto = false)
        {
            var circle = new Hold
            {
                StartTime = Time.Current + 1000,
                EndTime = Time.Current + 1000 + duration,
                Position = new Vector2(0, -66),
                EndPosition = new Vector2(0, -296.5f),
                Angle = 0f,
                NoteColor = Color4.Crimson,
            };

            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { });

            Add(new DrawableHold(circle)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Depth = depthIndex++,
                Auto = auto
            });
        }
    }
}
