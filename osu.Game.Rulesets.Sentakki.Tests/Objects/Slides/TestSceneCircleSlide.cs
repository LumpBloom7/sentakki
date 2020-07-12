using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Lines;
using osu.Framework.Testing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Rulesets.Objects.Types;
using osuTK;


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
        protected override List<PathControlPoint> CreatePattern() => SlidePaths.GenerateCirclePattern(EndPath, clockwise ? 1 : -1);
    }
}