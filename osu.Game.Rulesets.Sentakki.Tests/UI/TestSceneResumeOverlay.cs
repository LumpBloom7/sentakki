using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.UI
{
    public class TestSceneResumeOverlay : OsuTestScene
    {
        public TestSceneResumeOverlay()
        {
            CursorContainer cursor;
            ResumeOverlay resume;

            Children = new Drawable[]
            {
                cursor = new CursorContainer(),
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