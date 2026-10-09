using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects;

[TestFixture]
public partial class TestSceneTouchHold : OsuTestScene
{
    private readonly Container content;
    protected override Container<Drawable> Content => content;

    private int depthIndex;

    public TestSceneTouchHold()
    {
        base.Content.Add(content = new SentakkiInputManager(new SentakkiRuleset().RulesetInfo));
    }

    public static bool[] ObjectFlagsSource =
    [
        false,
        true
    ];

    [TestCaseSource(nameof(ObjectFlagsSource))]
    public void TestHolds(bool breakState)
    {
        testSingle(0, false, breakState);
        testSingle(0, true, breakState);

        testSingle(200, false, breakState);
        testSingle(200, true, breakState);

        testSingle(1000, false, breakState);
        testSingle(1000, true, breakState);

        testSingle(1000, false, breakState);
        testSingle(1000, true, breakState);
    }

    private void testSingle(double duration, bool auto = false, bool breakState = false)
    {
        DrawableTouchHold touchHold = null!;
        AddStep($"Create Hit Object ({duration:0.} ms)", () => touchHold = createHitObject(duration, auto, breakState));
        AddUntilStep("Wait until object judged", () => touchHold.AllJudged);

        if (auto)
        {
            AddAssert("Hold head is perfectly hit", () => touchHold.Head.Result.Type is HitResult.Perfect);
            AddAssert("Hold tail is perfectly hit", () => touchHold.Result.Type is HitResult.Perfect);
        }
        else
        {
            AddAssert("Hold head is missed", () => touchHold.Head.Result.Type is HitResult.Miss);
            AddAssert("Hold tail is missed", () => touchHold.Result.Type is HitResult.Miss);
        }
    }


    private DrawableTouchHold createHitObject(double duration, bool auto = false, bool breakState = false)
    {
        var circle = new TouchHold
        {
            StartTime = Time.Current + 1000,
            Duration = duration,
            Samples =
            [
                new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
            ],
            Break = breakState
        };

        if (breakState)
            circle.ColourPalette = TouchHold.BREAK_PALETTE;

        circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

        var dth = new DrawableTouchHold(circle)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Depth = depthIndex++,
            Auto = auto
        };

        Add(dth);

        return dth;
    }
    private void testSingle(bool auto = false, bool breakState = false)
    {
        var circle = new TouchHold
        {
            StartTime = Time.Current + 1000,
            Duration = 5000,
            Samples =
            [
                new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
            ],
            Break = breakState
        };

        if (breakState)
            circle.ColourPalette = TouchHold.BREAK_PALETTE;

        circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

        Add(new DrawableTouchHold(circle)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Depth = depthIndex++,
            Auto = auto
        });
    }

    protected override Ruleset CreateRuleset() => new SentakkiRuleset();
}
