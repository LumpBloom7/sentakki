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
    public partial class TestSceneBreakNote : OsuTestScene
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        protected override Ruleset CreateRuleset() => new SentakkiRuleset();
        private int depthIndex;

        public TestSceneBreakNote()
        {
            base.Content.Add(content = new SentakkiInputManager(new SentakkiRuleset().RulesetInfo));

            AddStep("Miss Single", () => testSingle());
            AddStep("Hit Single", () => testSingle(true));
            AddUntilStep("Wait for object despawn", () => !Children.Any(h => h is DrawableSentakkiHitObject sentakkiHitObject && sentakkiHitObject.AllJudged == false));
        }

        private void testSingle(bool auto = false)
        {
            var circle = new Tap
            {
                Break = true,
                StartTime = Time.Current + 1000,

                // No post-process is run
                NoteColour = Color4.OrangeRed,
            };

            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            Add(new DrawableTap(circle)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Depth = depthIndex++,
                Auto = auto
            });
        }
    }
}
