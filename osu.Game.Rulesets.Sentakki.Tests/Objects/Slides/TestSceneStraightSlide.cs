using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides
{
    public class TestSceneStraightSlide : TestSceneSlide
    {
        protected override List<PathControlPoint> CreatePattern() => SlidePaths.GenerateStraightPattern(EndPath);
    }
}