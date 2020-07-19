using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides
{
    public class TestSceneVSlide : TestSceneSlide
    {
        protected override List<PathControlPoint> CreatePattern() => SlidePaths.GenerateVPattern(EndPath);
    }
}