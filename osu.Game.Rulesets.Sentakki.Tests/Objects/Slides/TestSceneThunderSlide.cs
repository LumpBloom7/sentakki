using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides
{
    public class TestSceneThunderSlide : TestSceneSlide
    {
        private bool mirrored = false;

        public TestSceneThunderSlide()
        {
            AddToggleStep("Mirrored", b =>
            {
                mirrored = b;
                RefreshSlide();
            });
        }
        protected override SentakkiSlidePath CreatePattern() => SlidePaths.GenerateThunderPattern(mirrored);
    }
}
