using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides
{
    public class TestSceneThunderSlide : TestSceneSlide
    {
        private bool mirrored;

        public TestSceneThunderSlide()
        {
            AddToggleStep("Mirrored", b =>
            {
                mirrored = b;
                RefreshSlide();
            });
        }
        protected override SentakkiSlidePath CreatePattern() => SlidePaths.GenerateThunderPattern(StartPath, mirrored);
    }
}
