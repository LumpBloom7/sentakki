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
    public partial class TestSceneCircleChaining : OsuTestScene
    {
        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        private bool mirrored;

        private readonly SlideVisual slide;
        private readonly Container nodes;

        [Cached]
        private readonly DrawablePool<SlideChevron> chevronPool = null!;

        [Cached]
        private readonly SlideFanChevrons fanChevrons = null!;

        public TestSceneCircleChaining()
        {
            Add(chevronPool = new DrawablePool<SlideChevron>(62));
            Add(fanChevrons = new SlideFanChevrons());

            Add(new SentakkiRing
            {
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(SentakkiPlayfield.RINGSIZE)
            });

            Add(slide = new SlideVisual());

            AddToggleStep("Mirrored second part", b =>
            {
                mirrored = b;
                RefreshSlide();
            });

            AddStep("Perform entry animation", () => slide.PerformEntryAnimation(1000));
            AddWaitStep("Wait for transforms", 5);

            AddStep("Perform exit animation", () => slide.PerformExitAnimation(1000));
            AddWaitStep("Wait for transforms", 5);

            Add(nodes = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        protected SentakkiSlidePath CreatePattern()
        {
            var pathParameters = new[]{
                new SlideBodyPart(SlidePaths.PathShapes.Circle, endOffset: 4, false),
                new SlideBodyPart(SlidePaths.PathShapes.Circle, endOffset: 4, mirrored),
            };

            return SlidePaths.CreateSlidePath(pathParameters);
        }
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
                    Size = new Vector2(10),
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
