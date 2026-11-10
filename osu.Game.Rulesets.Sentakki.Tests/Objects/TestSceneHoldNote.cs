using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects;

[TestFixture]
public partial class TestSceneHoldNote : SentakkiSkinnableTestScene
{
    private int depthIndex;

    private void addStep(string title, Action action)
    {
        AddStep(title, action);
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
        testSingle("Miss Insane Short", 100, false, breakState, ex);
        testSingle("Hit Insane Short", 100, true, breakState, ex);
        testSingle("Miss Very Short", 200, false, breakState, ex);
        testSingle("Hit Very Short", 200, true, breakState, ex);
        testSingle("Miss Short", 500, false, breakState, ex);
        testSingle("Hit Short", 500, true, breakState, ex);
        testSingle("Miss Medium", 750, false, breakState, ex);
        testSingle("Hit Medium", 750, true, breakState, ex);
        testSingle("Miss Long", 1000, false, breakState, ex);
        testSingle("Hit Long", 1000, true, breakState, ex);
        testSingle("Miss Very Long", 3000, false, breakState, ex);
        testSingle("Hit Very Long", 3000, true, breakState, ex);
    }

    [Resolved]
    private GameHost host { get; set; } = null!;

    private void testSingle(string title, double duration, bool auto = false, bool breakState = false, bool ex = false)
    {
        AddStep($"Create Hit Object ({duration:0.} ms)", () => createHitObject(duration, auto, breakState, ex));
        AddUntilStep("Wait for object despawn", () =>
            !CreatedDrawables.Any(h => h is DrawableSentakkiHitObject { AllJudged: false }));

        if (host is HeadlessGameHost)
            return;

        if (auto)
        {
            AddAssert("Hold head is perfectly hit", () => CreatedDrawables.OfType<DrawableHold>()
                                                                          .All(h => h.Head.Result.Type is HitResult.Perfect));
            AddAssert("Hold tail is perfectly hit", () => CreatedDrawables.OfType<DrawableHold>()
                                                                          .All(h => h.Result.Type is HitResult.Perfect));
        }
        else
        {
            AddAssert("Hold head is missed", () => CreatedDrawables.OfType<DrawableHold>()
                                                                   .All(h => h.Head.Result.Type is HitResult.Miss));
            AddAssert("Hold head is missed", () => CreatedDrawables.OfType<DrawableHold>()
                                                                   .All(h => h.Result.Type is HitResult.Miss));
        }
    }

    private void createHitObject(double duration, bool auto = false, bool breakState = false, bool ex = false)
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

        SetContents(_ => new DrawableHold(circle)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Depth = depthIndex++,
            Auto = auto
        });
    }
}
