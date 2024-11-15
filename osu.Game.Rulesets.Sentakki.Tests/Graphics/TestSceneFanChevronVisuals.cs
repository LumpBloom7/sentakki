using System;
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

namespace osu.Game.Rulesets.Sentakki.Tests.Graphics
{
    [TestFixture]
    public partial class TestSceneFanChevronVisual : OsuGridTestScene
    {
        protected override Ruleset CreateRuleset() => new SentakkiRuleset();
        private readonly SlideVisual slide;

        private readonly Container fanChevContainer;

        [Cached]
        private readonly DrawablePool<SlideChevron> chevronPool;
        public TestSceneFanChevronVisual() : base(1, 2)
        {
            Cell(0).Add(chevronPool = new DrawablePool<SlideChevron>(62));
            Cell(0).Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White
            });
            Cell(0).Add(new SentakkiRing
            {
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(SentakkiPlayfield.RINGSIZE)
            });
            Cell(0).Add(slide = new SlideVisual());
            Cell(0).Add(new SentakkiRing
            {
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(SentakkiPlayfield.RINGSIZE)
            });
            Cell(1).Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White
            });
            Cell(1).Add(new SentakkiRing
            {
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(SentakkiPlayfield.RINGSIZE)
            });
            float rot = SentakkiExtensions.GetRotationForLane(4);
            Cell(1).Add(fanChevContainer = new Container()
            {
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(600),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Rotation = rot,
            });

            Vector2 lineStart = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0);
            Vector2 middleLineEnd = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 4);
            Vector2 middleLineDelta = middleLineEnd - lineStart;

            for (int i = 0; i < 11; ++i)
            {
                float progress = (i + 2f) / 12f;

                float scale = progress - (2f / 12);
                var middlePosition = lineStart + (middleLineDelta * progress);

                float t = 6.5f + (2.5f * scale);

                float chevWidth = MathF.Abs(lineStart.X - middlePosition.X);

                (float sin, float cos) = MathF.SinCos((-135 + 90f) / 180f * MathF.PI);

                Vector2 secondPoint = new Vector2(sin, -cos) * chevWidth;
                Vector2 one = new Vector2(chevWidth, 0);

                var middle = (one + secondPoint) * 0.5f;
                float h = (middle - Vector2.Zero).Length + (t * 3);

                float w = (secondPoint - one).Length;

                const float safe_space_ratio = (570 / 600f) * 600;

                float y = safe_space_ratio * scale;

                var fanChev = new DrawableChevron()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = w + 30,
                    Height = h + 30,
                    Thickness = t,
                    Y = -y + 300 - 50,
                    FanChevron = true,
                };

                fanChevContainer.Add(fanChev);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            slide.Path = SlidePaths.CreateSlidePath(new SlideBodyPart(SlidePaths.PathShapes.Fan, 4, false));
        }
    }
}
