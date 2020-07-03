using System.Collections.Generic;
using System;
using System.Collections;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Lines;
using osu.Framework.Testing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Rulesets.Objects.Types;
using osuTK;
using NUnit.Framework;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects.Slides
{
    [TestFixture]
    public abstract class TestSceneSlide : TestScene
    {
        private float progress;
        protected int StartPath;
        protected int EndPath;

        private readonly SmoothPath slide;

        public TestSceneSlide()
        {
            Add(new SentakkiRing());

            Add(slide = new SmoothPath
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                PathRadius = 2.5f,
                AutoSizeAxes = Axes.None,
                Size = new Vector2(600)
            });

            AddSliderStep("Start Path", 0, 7, 0, p =>
            {
                StartPath = p;
                refreshSlide();
            });
            AddSliderStep("End Path", 0, 7, 4, p =>
            {
                EndPath = p;
                refreshSlide();
            });
            AddSliderStep("Progress", 0.0f, 1.0f, 0.0f, p =>
            {
                progress = p;
                refreshSlide();
            });
        }
        protected abstract List<PathControlPoint> CreatePattern();

        private void refreshSlide()
        {
            List<Vector2> vertices = new List<Vector2>();
            var path = new SliderPath(CreatePattern().ToArray());
            path.GetPathToProgress(vertices, progress, 1);
            slide.Vertices = vertices;
        }
    }
}