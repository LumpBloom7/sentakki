using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Rulesets.Sentakki.Skinning.Common;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects;

[TestFixture]
public partial class TestSceneSlideFan : SentakkiSkinnableTestScene
{
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

    [TestCaseSource(nameof(ObjectFlagsSource))]
    public void TestSlideFan(bool headBreak, bool headEx, bool bodyBreak, bool bodyEx)
    {
        AddStep("Miss Single", () => testSingle(2000, false, headBreak, headEx, bodyBreak, bodyEx));
        AddStep("Hit Single", () => testSingle(2000, true, headBreak, headEx, bodyBreak, bodyEx));
        AddUntilStep("Wait for object despawn", () => !Children.Any(h => h is DrawableSentakkiHitObject hitObject && hitObject.AllJudged == false));
    }

    private void testSingle(double duration, bool auto = false, bool headBreak = false, bool headEx = false, bool bodyBreak = false, bool bodyEx = false)
    {
        var slide = new Slide
        {
            Break = headBreak,
            Ex = headEx,
            SlideInfoList =
            [
                new SlideBodyInfo
                {
                    Segments = [new SlideSegment(PathShape.Fan, 4, false)],
                    WaitDuration = 1000,
                    Duration = duration,
                    Break = bodyBreak,
                    Ex = bodyEx
                }
            ],
            StartTime = Time.Current + 1000,
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
                Auto = auto
            };

            foreach (DrawableSentakkiHitObject nested in dSlide.NestedHitObjects.OfType<DrawableSentakkiHitObject>())
            {
                foreach (DrawableSentakkiHitObject nested2 in nested.NestedHitObjects.OfType<DrawableSentakkiHitObject>())
                    nested2.Auto = auto;
            }

            return new SlideTestContext(dSlide);
        });
    }

    private partial class SlideTestContext : Container
    {
        [Cached]
        private SlideChevronProvider chevronPool = null!;

        public SlideTestContext(Drawable drawable)
        {
            RelativeSizeAxes = Axes.Both;

            AddInternal(chevronPool = new SlideChevronProvider());
            AddInternal(drawable);
        }
    }

    private void addStep(string title, Action action)
    {
        AddStep(title, action);
        AddUntilStep("Wait for object despawn", () => !CreatedDrawables.Any(h => h is DrawableSentakkiHitObject hitObject && hitObject.AllJudged == false));
    }
}
