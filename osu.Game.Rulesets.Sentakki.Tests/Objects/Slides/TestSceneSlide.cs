using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides
{
    [TestFixture]
    public abstract class TestSceneSlide : OsuTestScene
    {
        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        protected int StartPath;
        protected int EndPath;

        private readonly SlideVisual slide;
        private readonly Container nodes;

        [Cached]
        private readonly DrawablePool<SlideVisual.SlideChevron> chevronPool;

        public TestSceneSlide()
        {
            Add(chevronPool = new DrawablePool<SlideVisual.SlideChevron>(62));

            Add(new SentakkiRing()
            {
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(SentakkiPlayfield.RINGSIZE)
            });
            Add(slide = new SlideVisual());

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

            AddStep("Perform entry animation", () => slide.PerformEntryAnimation(1000));
            AddWaitStep("Wait for transforms", 5);

            AddStep("Perform exit animation", () => slide.PerformExitAnimation(1000));
            AddWaitStep("Wait for transforms", 5);

            Add(nodes = new Container()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        protected abstract SentakkiSlidePath CreatePattern();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            RefreshSlide();
        }

        protected void RefreshSlide()
        {
            slide.Path = CreatePattern();
            nodes.Clear();
            foreach (var node in slide.Path.SlideSegments.SelectMany(s => s.ControlPoints))
            {
                nodes.Add(new CircularContainer
                {
                    Size = new osuTK.Vector2(10),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Position = node.Position,
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
