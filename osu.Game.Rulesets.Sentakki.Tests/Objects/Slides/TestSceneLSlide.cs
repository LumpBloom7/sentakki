using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides
{
    public class TestSceneLSlide : TestSceneSlide
    {
        private bool mirrored = false;

        public TestSceneLSlide()
        {
            AddToggleStep("Mirrored", b =>
            {
                mirrored = b;
                RefreshSlide();
            });
        }

        protected override SentakkiSlidePath CreatePattern() => SlidePaths.GenerateLPattern(EndPath, mirrored);
    }
}