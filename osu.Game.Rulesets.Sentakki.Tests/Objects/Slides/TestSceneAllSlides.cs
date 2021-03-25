using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides
{
    [TestFixture]
    public class TestSceneAllSlides : OsuTestScene
    {
        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        private int id;
        private bool mirrored;
        private readonly SlideVisual slide;
        private readonly Container nodes;

        public TestSceneAllSlides()
        {
            Add(new SentakkiRing());
            Add(slide = new SlideVisual());

            AddSliderStep("Path ID", 0, SlidePaths.VALIDPATHS.Count - 1, 0, p =>
            {
                id = p;
                RefreshSlide();
            });

            AddToggleStep("Mirrored", b =>
            {
                mirrored = b;
                RefreshSlide();
            });

            Add(nodes = new Container()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        protected SentakkiSlidePath CreatePattern() => SlidePaths.GetSlidePath(id, mirrored);

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
