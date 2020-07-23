using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides
{
    public class TestSceneCircleSlide : TestSceneSlide
    {
        private bool clockwise = false;
        public TestSceneCircleSlide()
        {
            AddToggleStep("Clockwise", b =>
            {
                clockwise = b;
                RefreshSlide();
            });
        }
        protected override SentakkiSlidePath CreatePattern() => SlidePaths.GenerateCirclePattern(EndPath, clockwise ? 1 : -1);
    }
}