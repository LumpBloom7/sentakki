using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects;

[TestFixture]
public partial class TestSceneHoldNote : SentakkiSkinnableTestScene
{
    private int depthIndex;

    private void addStep(string title, Action action)
    {
        AddStep(title, action);
        AddUntilStep("Wait for object despawn", () => !CreatedDrawables.Any(h => h is DrawableSentakkiHitObject sentakkiHitObject && sentakkiHitObject.AllJudged == false));
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
        addStep("Miss Insane Short", () => testSingle(100, false, breakState, ex));
        addStep("Hit Insane Short", () => testSingle(100, true, breakState, ex));
        addStep("Miss Very Short", () => testSingle(200, false, breakState, ex));
        addStep("Hit Very Short", () => testSingle(200, true, breakState, ex));
        addStep("Miss Short", () => testSingle(500, false, breakState, ex));
        addStep("Hit Short", () => testSingle(500, true, breakState, ex));
        addStep("Miss Medium", () => testSingle(750, false, breakState, ex));
        addStep("Hit Medium", () => testSingle(750, true, breakState, ex));
        addStep("Miss Long", () => testSingle(1000, false, breakState, ex));
        addStep("Hit Long", () => testSingle(1000, true, breakState, ex));
        addStep("Miss Very Long", () => testSingle(3000, false, breakState, ex));
        addStep("Hit Very Long", () => testSingle(3000, true, breakState, ex));
    }

    private void testSingle(double duration, bool auto = false, bool breakState = false, bool ex = false)
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
