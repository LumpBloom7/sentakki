using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    [TestFixture]
    public partial class TestSceneSlideNote : OsuTestScene
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        private int depthIndex;

        [Cached]
        private readonly DrawablePool<SlideChevron> chevronPool;

        public static bool[][] ObjectFlagsSource = [
            [false, false, false, false],
            [true, false, false, false],
            [false, true, false, false],
            [true, true, false, false],
            [false, false, true, false],
            [false, false, false, true],
            [false, false, true, true],
        ];

        public TestSceneSlideNote()
        {
            base.Content.Add(content = new SentakkiInputManager(new SentakkiRuleset().RulesetInfo));
            Add(new SentakkiRing
            {
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(SentakkiPlayfield.RINGSIZE),
                Rotation = -22.5f
            });

            Add(chevronPool = new DrawablePool<SlideChevron>(62));
        }

        [TestCaseSource(nameof(ObjectFlagsSource))]
        public void TestSlides(bool headBreak, bool headEX, bool bodyBreak, bool bodyEx)
        {

            AddStep("Miss Single", () => testSingle(2000, false, headBreak, headEX, bodyBreak, bodyEx));
            AddStep("Hit Single", () => testSingle(2000, true, headBreak, headEX, bodyBreak, bodyEx));
            AddUntilStep("Wait for object despawn", () => !Children.Any(h => (h is DrawableSentakkiHitObject hitObject) && hitObject.AllJudged == false));

            AddStep("Miss chain", () => testChain(5000, false, headBreak, headEX, bodyBreak, bodyEx));
            AddStep("Hit chain", () => testChain(5000, true, headBreak, headEX, bodyBreak, bodyEx));
            AddUntilStep("Wait for object despawn", () => !Children.Any(h => (h is DrawableSentakkiHitObject hitObject) && hitObject.AllJudged == false));

            AddStep("Miss chain with Fan", () => testChainWithFan(6000, false, headBreak, headEX, bodyBreak, bodyEx));
            AddStep("Hit chain with Fan", () => testChainWithFan(6000, true, headBreak, headEX, bodyBreak, bodyEx));
            AddUntilStep("Wait for object despawn", () => !Children.Any(h => (h is DrawableSentakkiHitObject hitObject) && hitObject.AllJudged == false));
        }

        private void testSingle(double duration, bool auto = false, bool headBreak = false, bool headEX = false, bool bodyBreak = false, bool bodyEx = false)
        {
            var slide = new Slide
            {
                //Break = true,
                SlideInfoList = new List<SlideBodyInfo>
                {
                    new SlideBodyInfo
                    {
                        SlidePathParts = new[] { new SlideBodyPart(SlidePaths.PathShapes.Circle, 0, false) },
                        Duration = 1000,
                        Break = bodyBreak,
                    },
                    new SlideBodyInfo
                    {
                        SlidePathParts = new[] { new SlideBodyPart(SlidePaths.PathShapes.Straight, 4, false) },
                        Duration = 1500,
                        Break = bodyBreak,
                    },
                    new SlideBodyInfo
                    {
                        SlidePathParts = new[] { new SlideBodyPart(SlidePaths.PathShapes.Cup, 2, false) },
                        Duration = 2000,
                        Break = bodyBreak,
                    }
                },
                StartTime = Time.Current + 1000,
                Ex = headEX,
                Break = headBreak,
            };

            slide.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            if (headBreak)
                slide.SlideTap.NoteColour = Color4.OrangeRed;
            if (bodyBreak)
            {
                foreach (var body in slide.SlideBodies)
                    body.NoteColour = Color4.OrangeRed;
            }

            DrawableSlide dSlide;

            Add(dSlide = new DrawableSlide(slide)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Depth = depthIndex++,
                Auto = auto
            });

            foreach (DrawableSentakkiHitObject nested in dSlide.NestedHitObjects)
            {
                foreach (DrawableSentakkiHitObject nested2 in nested.NestedHitObjects.OfType<DrawableSentakkiHitObject>())
                    nested2.Auto = auto;
            }
        }

        private void testChain(double duration, bool auto = false, bool headBreak = false, bool headEX = false, bool bodyBreak = false, bool bodyEx = false)
        {
            var slide = new Slide
            {
                //Break = true,
                SlideInfoList = new List<SlideBodyInfo>
                {
                    new SlideBodyInfo
                    {
                        SlidePathParts = new[]
                        {
                            new SlideBodyPart(SlidePaths.PathShapes.Cup, 2, false),
                            new SlideBodyPart(SlidePaths.PathShapes.Cup, 2, false),
                            new SlideBodyPart(SlidePaths.PathShapes.Cup, 2, false),
                            new SlideBodyPart(SlidePaths.PathShapes.Cup, 2, false),
                        },
                        Duration = duration,
                        Break = bodyBreak,
                    },
                },
                StartTime = Time.Current + 1000,
                Break = headBreak,
                Ex = headEX
            };

            slide.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            if (headBreak)
                slide.SlideTap.NoteColour = Color4.OrangeRed;
            if (bodyBreak)
            {
                foreach (var body in slide.SlideBodies)
                    body.NoteColour = Color4.OrangeRed;
            }

            DrawableSlide dSlide;

            Add(dSlide = new DrawableSlide(slide)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Depth = depthIndex++,
                Auto = auto
            });

            foreach (DrawableSentakkiHitObject nested in dSlide.NestedHitObjects)
            {
                foreach (DrawableSentakkiHitObject nested2 in nested.NestedHitObjects.OfType<DrawableSentakkiHitObject>())
                    nested2.Auto = auto;
            }
        }

        private void testChainWithFan(double duration, bool auto = false, bool headBreak = false, bool headEX = false, bool bodyBreak = false, bool bodyEx = false)
        {
            var slide = new Slide
            {
                //Break = true,
                SlideInfoList = new List<SlideBodyInfo>
                {
                    new SlideBodyInfo
                    {
                        SlidePathParts = new[]
                        {
                            new SlideBodyPart(SlidePaths.PathShapes.Cup, 2, false),
                            new SlideBodyPart(SlidePaths.PathShapes.Cup, 2, false),
                            new SlideBodyPart(SlidePaths.PathShapes.Cup, 2, false),
                            new SlideBodyPart(SlidePaths.PathShapes.Cup, 2, false),
                            new SlideBodyPart(SlidePaths.PathShapes.Fan, 4, false),
                        },
                        Duration = duration,
                        Break = bodyBreak,
                    },
                },
                StartTime = Time.Current + 1000,
                Break = headBreak,
                Ex = headEX
            };

            slide.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            if (headBreak)
                slide.SlideTap.NoteColour = Color4.OrangeRed;
            if (bodyBreak)
            {
                foreach (var body in slide.SlideBodies)
                    body.NoteColour = Color4.OrangeRed;
            }

            DrawableSlide dSlide;

            Add(dSlide = new DrawableSlide(slide)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Depth = depthIndex++,
                Auto = auto
            });

            foreach (DrawableSentakkiHitObject nested in dSlide.NestedHitObjects)
            {
                foreach (DrawableSentakkiHitObject nested2 in nested.NestedHitObjects.OfType<DrawableSentakkiHitObject>())
                    nested2.Auto = auto;
            }
        }
    }
}
