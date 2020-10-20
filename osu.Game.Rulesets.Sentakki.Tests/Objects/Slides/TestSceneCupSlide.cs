using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides
{
    public class TestSceneCupSlide : TestSceneSlide
    {
        private bool mirrored = false;

        public TestSceneCupSlide()
        {
            AddToggleStep("Mirrored", b =>
            {
                mirrored = b;
                RefreshSlide();
            });
        }

        protected override SentakkiSlidePath CreatePattern() => SlidePaths.GenerateCupPattern(EndPath, mirrored);
    }
}
