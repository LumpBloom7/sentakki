using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    [TestFixture]
    public partial class TestSceneHoldNote : OsuTestScene
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;
        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        private int depthIndex;

        public TestSceneHoldNote()
        {
            base.Content.Add(content = new SentakkiInputManager(new SentakkiRuleset().RulesetInfo));
        }

        public static bool[][] ObjectFlagsSource = [
            [false, false],
            [true, false],
            [false, true],
            [true, true],
        ];

        [TestCaseSource(nameof(ObjectFlagsSource))]
        public void TestHolds(bool breakState, bool ex)
        {
            AddStep("Miss Insane Short", () => testSingle(100, false, breakState, ex));
            AddStep("Hit Insane Short", () => testSingle(100, true, breakState, ex));
            AddStep("Miss Very Short", () => testSingle(200, false, breakState, ex));
            AddStep("Hit Very Short", () => testSingle(200, true, breakState, ex));
            AddStep("Miss Short", () => testSingle(500, false, breakState, ex));
            AddStep("Hit Short", () => testSingle(500, true, breakState, ex));
            AddStep("Miss Medium", () => testSingle(750, false, breakState, ex));
            AddStep("Hit Medium", () => testSingle(750, true, breakState, ex));
            AddStep("Miss Long", () => testSingle(1000, false, breakState, ex));
            AddStep("Hit Long", () => testSingle(1000, true, breakState, ex));
            AddStep("Miss Very Long", () => testSingle(3000, false, breakState, ex));
            AddStep("Hit Very Long", () => testSingle(3000, true, breakState, ex));
            AddUntilStep("Wait for object despawn", () => !Children.Any(h => h is DrawableSentakkiHitObject sentakkiHitObject && sentakkiHitObject.AllJudged == false));
        }

        private void testSingle(double duration, bool auto = false, bool breakState = false, bool ex = false)
        {
            var circle = new Hold
            {
                StartTime = Time.Current + 1000,
                EndTime = Time.Current + 1000 + duration,
                Break = breakState,
                Ex = ex
            };

            if (breakState)
                circle.NoteColour = Color4.OrangeRed;

            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

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
