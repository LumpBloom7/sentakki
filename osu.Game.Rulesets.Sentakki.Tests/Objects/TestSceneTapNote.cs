using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects;

public partial class TestSceneTapNote : SentakkiSkinnableTestScene
{
    private int depthIndex;

    public static bool[][] ObjectFlagsSource =
    [
        [false, false],
        [true, false],
        [false, true],
        [true, true],
    ];


    [TestCaseSource(nameof(ObjectFlagsSource))]
    public void PerformNoteTest(bool breakState = false, bool ex = false)
    {
        addStep("Miss Single", () => testSingle(false, breakState, ex));
        addStep("Hit Single", () => testSingle(true, breakState, ex));
    }

    private void addStep(string title, Action action)
    {
        AddStep(title, action);
        AddUntilStep("Wait for object despawn", () => !CreatedDrawables.Any(h => h is DrawableSentakkiHitObject sentakkiHitObject && sentakkiHitObject.AllJudged == false));
    }

    private void testSingle(bool auto = false, bool breakState = false, bool ex = false)
    {
        var circle = new Tap
        {
            StartTime = Time.Current + 1000,
            Break = breakState,
            Ex = ex
        };

        if (breakState)
            circle.NoteColour = Color4.OrangeRed;

        circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

        SetContents(_ => new DrawableTap(circle)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Depth = depthIndex++,
            Auto = auto
        });
    }
}
