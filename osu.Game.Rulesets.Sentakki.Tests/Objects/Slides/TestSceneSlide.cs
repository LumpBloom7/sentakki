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
    public abstract partial class TestSceneSlide : OsuTestScene
    {
        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        private int startPath;
        private int endPath;

        private bool mirrored;

        protected abstract SlidePaths.PathShapes PathShape { get; }

        private readonly SlideVisual slide;
        private readonly Container nodes;

        [Cached]
        private readonly DrawablePool<SlideChevron> chevronPool;

        protected TestSceneSlide()
        {
            Add(chevronPool = new DrawablePool<SlideChevron>(62));

            Add(new SentakkiRing
            {
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(SentakkiPlayfield.RINGSIZE)
            });

            Add(slide = new SlideVisual());

            AddSliderStep("Start lane", 0, 7, 0, p =>
            {
                startPath = p;
                RefreshSlide();
            });
            AddSliderStep("End lane", 0, 7, 4, p =>
            {
                endPath = p;
                RefreshSlide();
            });
            AddSliderStep("Progress", 0.0f, 1.0f, 0.0f, p =>
            {
                slide.Progress = p;
                slide.UpdateChevronVisibility();
            });

            AddToggleStep("Mirrored", b =>
            {
                mirrored = b;
                RefreshSlide();
            });

            AddStep("Perform entry animation", () => performEntryAnimation(1000));
            AddWaitStep("Wait for transforms", 5);

            AddStep("Perform exit animation", () => performExitAnimation(1000));
            AddWaitStep("Wait for transforms", 5);

            Add(nodes = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        private double entryTime = double.MinValue;
        private double entryDuration = 0;

        private double exitTime = double.MaxValue;
        private double exitDuration = 0;

        protected override void Update()
        {
            base.Update();
            slide.UpdateSlideVisuals(entryTime, entryDuration, exitTime, exitDuration);
        }

        private void performEntryAnimation(double duration)
        {
            entryTime = Time.Current;
            entryDuration = duration;

            exitTime = double.MaxValue;
            exitDuration = 0;
        }
        private void performExitAnimation(double duration)
        {
            entryTime = double.MinValue;
            entryDuration = 0;

            exitTime = Time.Current;
            exitDuration = duration;
        }

        protected SentakkiSlidePath CreatePattern() => SlidePaths.CreateSlidePath(startPath, new SlideBodyPart(PathShape, endPath, mirrored));

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
