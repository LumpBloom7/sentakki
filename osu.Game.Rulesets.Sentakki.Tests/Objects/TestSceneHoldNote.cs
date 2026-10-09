using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects;

[TestFixture]
public partial class TestSceneHoldNote : OsuTestScene
{
    private readonly Container content;
    protected override Container<Drawable> Content => content;
    protected override Ruleset CreateRuleset() => new SentakkiRuleset();

    private int depthIndex;

    public TestSceneHoldNote()
    {
        base.Content.Add(content = new SentakkiInputManager(new SentakkiRuleset().RulesetInfo));
    }

    public static bool[][] ObjectFlagsSource =
    [
        [false, false],
        [true, false],
        [false, true],
        [true, true],
    ];

    [TestCaseSource(nameof(ObjectFlagsSource))]
    public void TestHolds(bool breakState, bool ex)
    {
        testSingle(0, false, breakState, ex);
        testSingle(0, true, breakState, ex);

        testSingle(200, false, breakState, ex);
        testSingle(200, true, breakState, ex);

        testSingle(1000, false, breakState, ex);
        testSingle(1000, true, breakState, ex);

        testSingle(1000, false, breakState, ex);
        testSingle(1000, true, breakState, ex);
    }

    private void testSingle(double duration, bool auto = false, bool breakState = false, bool ex = false)
    {
        DrawableHold hold = null!;
        AddStep($"Create Hit Object ({duration:0.} ms)", () => hold = createHitObject(duration, auto, breakState, ex));
        AddUntilStep("Wait until object judged", () => hold.AllJudged);

        if (auto)
        {
            AddAssert("Hold head is perfectly hit", () => hold.Head.Result.Type is HitResult.Perfect);
            AddAssert("Hold tail is perfectly hit", () => hold.Result.Type is HitResult.Perfect);
        }
        else
        {
            AddAssert("Hold head is missed", () => hold.Head.Result.Type is HitResult.Miss);
            AddAssert("Hold tail is missed", () => hold.Result.Type is HitResult.Miss);
        }
    }


    private DrawableHold createHitObject(double duration, bool auto = false, bool breakState = false, bool ex = false)
    {
        var circle = new Hold
        {
            StartTime = Time.Current + 1000,
            EndTime = Time.Current + 1000 + duration,
            Break = breakState,
            Ex = ex
        };

        if (breakState)
            circle.NoteColour = Color4.OrangeRed;

        circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

        DrawableHold drawable = new DrawableHold(circle)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Depth = depthIndex++,
            Auto = auto
        };

        Add(drawable);

        return drawable;
    }
}
