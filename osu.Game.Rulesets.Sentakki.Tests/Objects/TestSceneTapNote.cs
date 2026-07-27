using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects;

[TestFixture]
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
        AddStep("Miss Single", () => SetContents(_ => testSingle(false, breakState, ex)));
        AddUntilStep("Wait for object despawn", () => !CreatedDrawables.Any(h => h is DrawableSentakkiHitObject sentakkiHitObject && sentakkiHitObject.AllJudged == false));
        AddStep("Hit Single", () => SetContents(_ => testSingle(true, breakState, ex)));
        AddUntilStep("Wait for object despawn", () => !CreatedDrawables.Any(h => h is DrawableSentakkiHitObject sentakkiHitObject && sentakkiHitObject.AllJudged == false));
    }

    private Drawable testSingle(bool auto = false, bool breakState = false, bool ex = false)
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

        return new DrawableTap(circle)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Depth = depthIndex++,
            Auto = auto
        };
    }
}
