using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides
{
    [TestFixture]
    public class TestSceneFanSlide : OsuTestScene
    {
        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        protected int StartPath;
        protected int EndPath;

        private readonly SlideFanVisual slide;
        private readonly Container nodes;

        public TestSceneFanSlide()
        {
            Add(new SentakkiRing()
            {
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(SentakkiPlayfield.RINGSIZE)
            });

            Add(slide = new SlideFanVisual()
            {
                Rotation = 22.5f
            });

            AddSliderStep("Progress", 0.0f, 1.0f, 0.0f, p =>
            {
                slide.Progress = p;
            });

            Add(nodes = new Container()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            slide.Path = SlidePaths.GenerateStraightPattern(4);
        }
    }
}
