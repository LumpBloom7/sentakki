using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides
{
    public class TestSceneCircleSlide : TestSceneSlide
    {
        private bool clockwise;
        public TestSceneCircleSlide()
        {
            AddToggleStep("Clockwise", b =>
            {
                clockwise = b;
                RefreshSlide();
            });
        }
        protected override SentakkiSlidePath CreatePattern() => SlidePaths.GenerateCirclePattern(EndPath, clockwise ? RotationDirection.Clockwise : RotationDirection.CounterClockwise);
    }
}
