using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides
{
    public class TestSceneUSlide : TestSceneSlide
    {
        protected override SentakkiSlidePath CreatePattern() => SlidePaths.GenerateUPattern(EndPath);
    }
}