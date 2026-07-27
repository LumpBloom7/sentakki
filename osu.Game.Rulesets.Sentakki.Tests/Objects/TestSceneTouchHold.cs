using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects;

[TestFixture]
public partial class TestSceneTouchHold : SentakkiSkinnableTestScene
{
    public static bool[][] ObjectFlagsSource =
    [
        [false],
        [true],
    ];

    [TestCaseSource(nameof(ObjectFlagsSource))]
    public void TestTouchHold(bool breakState)
    {
        AddStep("Miss Single", () => testSingle(false, breakState));
        AddStep("Hit Single", () => testSingle(true, breakState));
        AddUntilStep("Wait for object despawn", () => !Children.Any(h => h is DrawableSentakkiHitObject sentakkiHitObject && sentakkiHitObject.AllJudged == false));
    }

    private void addStep(string title, Action action)
    {
        AddStep(title, action);
        AddUntilStep("Wait for object despawn", () => !CreatedDrawables.Any(h => h is DrawableSentakkiHitObject hitObject && hitObject.AllJudged == false));
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

        SetContents(_ => new DrawableTouchHold(circle)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Auto = auto
        });
    }
}
