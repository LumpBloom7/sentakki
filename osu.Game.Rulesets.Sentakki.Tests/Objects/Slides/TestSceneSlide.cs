using System.Collections.Generic;
using System;
using System.Collections;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Testing;
using osu.Game.Rulesets.Objects;
using osu.Framework.Graphics.Textures;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;
using NUnit.Framework;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides
{
    [TestFixture]
    public abstract class TestSceneSlide : OsuTestScene
    {
        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        protected int StartPath = 0;
        protected int EndPath;

        private readonly SlideBody slide;

        public TestSceneSlide()
        {
            Add(new SentakkiRing());

            Add(slide = new SlideBody()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(600),
                Path = new SliderPath(CreatePattern().ToArray())
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
        }
        protected abstract List<PathControlPoint> CreatePattern();

        protected void RefreshSlide()
        {
            slide.Path = new SliderPath(CreatePattern().ToArray());
        }
    }
}