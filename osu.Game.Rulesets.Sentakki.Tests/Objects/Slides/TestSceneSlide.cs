using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides
{
    [TestFixture]
    public abstract class TestSceneSlide : OsuTestScene
    {
        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        protected int StartPath = 0;
        protected int EndPath;

        private readonly SlideVisual slide;
        private readonly Container nodes;

        public TestSceneSlide()
        {
            Add(new SentakkiRing());

            Add(slide = new SlideVisual()
            {
                Path = CreatePattern().Path
            });

            AddSliderStep("Path offset", 0, 7, 0, p =>
            {
                slide.Rotation = 45 * p;
            });
            AddSliderStep("End Path", 0, 7, 4, p =>
            {
                EndPath = p;
                RefreshSlide();
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
        protected abstract SentakkiSlidePath CreatePattern();

        protected void RefreshSlide()
        {
            slide.Path = CreatePattern().Path;
            nodes.Clear();
            foreach (var node in slide.Path.ControlPoints)
            {
                nodes.Add(new CircularContainer
                {
                    Size = new osuTK.Vector2(10),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Position = node.Position.Value,
                    Masking = true,
                    Child = new Box
                    {
                        Colour = Color4.Green,
                        RelativeSizeAxes = Axes.Both
                    }
                });
            }
        }
    }
}
