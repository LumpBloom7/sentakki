using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects;

[TestFixture]
public partial class TestSceneSlideNote : SentakkiSkinnableTestScene
{

    private int depthIndex;

    [Cached]
    private readonly DrawablePool<SlideChevron> chevronPool;

    public static bool[][] ObjectFlagsSource =
    [
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
        Add(chevronPool = new DrawablePool<SlideChevron>(62));
    }

    [TestCaseSource(nameof(ObjectFlagsSource))]
    public void TestSlides(bool headBreak, bool headEx, bool bodyBreak, bool bodyEx)
    {
        addStep("Miss Single", () => testSingle(2000, false, headBreak, headEx, bodyBreak, bodyEx));
        addStep("Hit Single", () => testSingle(2000, true, headBreak, headEx, bodyBreak, bodyEx));

        addStep("Miss chain", () => testChain(5000, false, headBreak, headEx, bodyBreak, bodyEx));
        addStep("Hit chain", () => testChain(5000, true, headBreak, headEx, bodyBreak, bodyEx));

        addStep("Miss chain with Fan", () => testChainWithFan(6000, false, headBreak, headEx, bodyBreak, bodyEx));
        addStep("Hit chain with Fan", () => testChainWithFan(6000, true, headBreak, headEx, bodyBreak, bodyEx));
    }

    private void addStep(string title, Action action)
    {
        AddStep(title, action);
        AddUntilStep("Wait for object despawn", () => !CreatedDrawables.Any(h => h is DrawableSentakkiHitObject hitObject && hitObject.AllJudged == false));
    }

    private void testSingle(double duration, bool auto = false, bool headBreak = false, bool headEx = false, bool bodyBreak = false, bool bodyEx = false)
    {
        var slide = new Slide
        {
            //Break = true,
            SlideInfoList =
            [
                new SlideBodyInfo
                {
                    WaitDuration = 100,
                    Segments = [new SlideSegment(PathShape.Circle, 0, false)],
                    Duration = 1000,
                    Break = bodyBreak,
                    Ex = bodyEx
                },

                new SlideBodyInfo
                {
                    WaitDuration = 100,
                    Segments = [new SlideSegment(PathShape.Straight, 4, false)],
                    Duration = 1500,
                    Break = bodyBreak,
                    Ex = bodyEx
                },

                new SlideBodyInfo
                {
                    WaitDuration = 100,
                    Segments = [new SlideSegment(PathShape.Cup, 2, false)],
                    Duration = 2000,
                    Break = bodyBreak,
                    Ex = bodyEx
                }
            ],
            StartTime = Time.Current + 1000,
            Ex = headEx,
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

        SetContents(_ =>
        {
            DrawableSlide dSlide = new DrawableSlide(slide)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Depth = depthIndex++,
                Auto = auto,
                Scale = new Vector2(0.3f),
            };

            foreach (DrawableSentakkiHitObject nested in dSlide.NestedHitObjects.OfType<DrawableSentakkiHitObject>())
            {
                foreach (DrawableSentakkiHitObject nested2 in nested.NestedHitObjects.OfType<DrawableSentakkiHitObject>())
                    nested2.Auto = auto;
            }

            return dSlide;
        });
    }

    private void testChain(double duration, bool auto = false, bool headBreak = false, bool headEx = false, bool bodyBreak = false, bool bodyEx = false)
    {
        var slide = new Slide
        {
            //Break = true,
            SlideInfoList =
            [
                new SlideBodyInfo
                {
                    Segments =
                    [
                        new SlideSegment(PathShape.Cup, 2, false),
                        new SlideSegment(PathShape.Cup, 2, false),
                        new SlideSegment(PathShape.Cup, 2, false),
                        new SlideSegment(PathShape.Cup, 2, false)
                    ],
                    WaitDuration = 100,
                    Duration = duration,
                    Break = bodyBreak,
                    Ex = bodyEx
                }
            ],
            StartTime = Time.Current + 1000,
            Break = headBreak,
            Ex = headEx
        };

        slide.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

        if (headBreak)
            slide.SlideTap.NoteColour = Color4.OrangeRed;

        if (bodyBreak)
        {
            foreach (var body in slide.SlideBodies)
                body.NoteColour = Color4.OrangeRed;
        }

        SetContents(_ =>
        {
            DrawableSlide dSlide = new DrawableSlide(slide)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Depth = depthIndex++,
                Auto = auto,
                Scale = new Vector2(0.3f),
            };

            foreach (DrawableSentakkiHitObject nested in dSlide.NestedHitObjects.OfType<DrawableSentakkiHitObject>())
            {
                foreach (DrawableSentakkiHitObject nested2 in nested.NestedHitObjects.OfType<DrawableSentakkiHitObject>())
                    nested2.Auto = auto;
            }

            return dSlide;
        });
    }

    private void testChainWithFan(double duration, bool auto = false, bool headBreak = false, bool headEx = false, bool bodyBreak = false, bool bodyEx = false)
    {
        var slide = new Slide
        {
            //Break = true,
            SlideInfoList =
            [
                new SlideBodyInfo
                {
                    Segments =
                    [
                        new SlideSegment(PathShape.Cup, 2, false),
                        new SlideSegment(PathShape.Cup, 2, false),
                        new SlideSegment(PathShape.Cup, 2, false),
                        new SlideSegment(PathShape.Cup, 2, false),
                        new SlideSegment(PathShape.Fan, 4, false)
                    ],
                    WaitDuration = 100,
                    Duration = duration,
                    Break = bodyBreak,
                    Ex = bodyEx
                }
            ],
            StartTime = Time.Current + 1000,
            Break = headBreak,
            Ex = headEx
        };

        slide.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

        if (headBreak)
            slide.SlideTap.NoteColour = Color4.OrangeRed;

        if (bodyBreak)
        {
            foreach (var body in slide.SlideBodies)
                body.NoteColour = Color4.OrangeRed;
        }

        SetContents(_ =>
        {
            DrawableSlide dSlide = new DrawableSlide(slide)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Depth = depthIndex++,
                Auto = auto,
                Scale = new Vector2(0.3f),
            };

            foreach (DrawableSentakkiHitObject nested in dSlide.NestedHitObjects.OfType<DrawableSentakkiHitObject>())
            {
                foreach (DrawableSentakkiHitObject nested2 in nested.NestedHitObjects.OfType<DrawableSentakkiHitObject>())
                    nested2.Auto = auto;
            }

            return dSlide;
        });
    }
}
