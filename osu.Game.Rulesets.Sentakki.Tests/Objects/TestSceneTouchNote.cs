using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects;

[TestFixture]
public partial class TestSceneTouchNote : SentakkiSkinnableTestScene
{
    [TestCaseSource(nameof(ObjectFlagsSource))]
    public void TestTouchNotes(bool breakState, bool ex)
    {
        addStep("Miss Single", () => testSingle(false, breakState, ex));
        addStep("Hit Single", () => testSingle(true, breakState, ex));
    }

    public static bool[][] ObjectFlagsSource =
    [
        [false, false],
        [true, false],
        [false, true],
        [true, true],
    ];

    private void addStep(string title, Action action)
    {
        AddStep(title, action);
        AddUntilStep("Wait for object despawn", () => !CreatedDrawables.Any(h => h is DrawableSentakkiHitObject hitObject && hitObject.AllJudged == false));
    }

    private void testSingle(bool auto = false, bool breakState = false, bool ex = false)
    {
        var circle = new Touch
        {
            StartTime = Time.Current + 1000,
            Position = Vector2.Zero,
            Break = breakState,
            Ex = ex
        };

        if (breakState)
            circle.NoteColour = Color4.OrangeRed;

        circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

        SetContents(_ => new DrawableTouch(circle)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Auto = auto
        });
    }
}
