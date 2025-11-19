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
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects;

[TestFixture]
public partial class TestSceneInvalidSlideProperties : OsuTestScene
{
    private readonly Container content;
    protected override Container<Drawable> Content => content;

    protected override Ruleset CreateRuleset() => new SentakkiRuleset();

    [Cached]
    private DrawablePool<SlideChevron> chevronPool;

    public TestSceneInvalidSlideProperties()
    {
        base.Content.Add(content = new SentakkiInputManager(new SentakkiRuleset().RulesetInfo));
        Add(chevronPool = new DrawablePool<SlideChevron>(62));

        AddStep("Test negative wait duration", testNegativeWaitDuration);
        AddStep("Test larger wait duration than slide body duration", testLargerWaitDurationThanDuration);
    }

    private void testNegativeWaitDuration()
    {
        var slideBody = new SlideBodyInfo
        {
            Segments = [new SlideSegment(PathShape.Straight, 4, false)],
            WaitDuration = -500,
            Duration = 1000
        };

        Assert.That(slideBody.WaitDuration == -500, "User set wait duration is -500");
        Assert.That(slideBody.EffectiveWaitDuration == 0, "Effective wait duration is 0");
        Assert.That(slideBody.EffectiveMovementDuration == slideBody.Duration, "Effective movement duration is the same as user set duration.");

        var slide = new Slide
        {
            SlideInfoList = [slideBody],
            StartTime = Time.Current + 1000
        };

        slide.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

        Add(new DrawableSlide(slide)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        });
    }

    private void testLargerWaitDurationThanDuration()
    {
        var slideBody = new SlideBodyInfo
        {
            Segments = [new SlideSegment(PathShape.Straight, 4, false)],
            WaitDuration = 5000,
            Duration = 1000
        };

        Assert.That(slideBody.WaitDuration == 5000, "User set wait duration is 5000");
        Assert.That(slideBody.EffectiveWaitDuration == slideBody.Duration, "Effective wait duration is clamped to slideBodyDuration.");
        Assert.That(slideBody.EffectiveMovementDuration == 0, "Effective movement duration is exactly 0.");

        var slide = new Slide
        {
            SlideInfoList = [slideBody],
            StartTime = Time.Current + 1000
        };

        slide.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

        Add(new DrawableSlide(slide)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        });
    }
}
