using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides
{
    public class TestSceneUSlide : TestSceneSlide
    {
        private bool reversed;

        public TestSceneUSlide()
        {
            AddToggleStep("Mirrored", b =>
            {
                reversed = b;
                RefreshSlide();
            });
        }

        protected override SentakkiSlidePath CreatePattern() => SlidePaths.GenerateUPattern(StartPath, EndPath, reversed);
    }
}
