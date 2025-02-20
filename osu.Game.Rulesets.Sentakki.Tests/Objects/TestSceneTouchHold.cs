using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    [TestFixture]
    public partial class TestSceneTouchHold : OsuTestScene
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private int depthIndex;

        public TestSceneTouchHold()
        {
            base.Content.Add(content = new SentakkiInputManager(new SentakkiRuleset().RulesetInfo));
        }

        public static bool[][] ObjectFlagsSource = [
            [false],
            [true],
        ];

        [TestCaseSource(nameof(ObjectFlagsSource))]
        public void TestTouchHold(bool breakState)
        {
            AddStep("Miss Single", () => testSingle(false, breakState));
            AddStep("Hit Single", () => testSingle(true, breakState));
            AddUntilStep("Wait for object despawn", () => !Children.Any(h => (h is DrawableSentakkiHitObject sentakkiHitObject) && sentakkiHitObject.AllJudged == false));
        }

        private void testSingle(bool auto = false, bool breakState = false)
        {
            var circle = new TouchHold
            {
                StartTime = Time.Current + 1000,
                Duration = 5000,
                Samples = new List<HitSampleInfo>
                {
                    new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
                },
                Break = breakState
            };

            if (breakState)
                circle.ColourPalette = TouchHold.BreakPalette;

            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            Add(new DrawableTouchHold(circle)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Depth = depthIndex++,
                Auto = auto
            });
        }

        protected override Ruleset CreateRuleset() => new SentakkiRuleset();
    }
}
