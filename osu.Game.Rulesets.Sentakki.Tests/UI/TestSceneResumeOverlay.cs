using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.UI
{
    public partial class TestSceneResumeOverlay : OsuTestScene
    {
        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        public TestSceneResumeOverlay()
        {
            GameplayCursorContainer cursor;
            ResumeOverlay resume;

            Children = new Drawable[]
            {
                cursor = new GameplayCursorContainer(),
                resume = new SentakkiResumeOverlay
                {
                    GameplayCursor = cursor
                }
            };

            AddStep("Show ResumeOverlay", () => resume.Show());
            AddAssert("Is overlay shown?", () => resume.State.Value == Visibility.Visible);
            AddUntilStep("Wait for countdown to end", () => resume.State.Value == Visibility.Hidden);
        }
    }
}
