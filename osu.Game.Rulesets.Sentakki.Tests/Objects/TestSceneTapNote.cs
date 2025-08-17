using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects;

public partial class TestSceneTapNote : OsuTestScene
{
    private Container content = null!;
    protected override Container<Drawable> Content => content;

    protected override Ruleset CreateRuleset() => new SentakkiRuleset();

    private int depthIndex;

    public static bool[][] ObjectFlagsSource =
    [
        [false, false],
        [true, false],
        [false, true],
        [true, true],
    ];

    [BackgroundDependencyLoader]
    private void load()
    {
        base.Content.Add(content = new SentakkiInputManager(new SentakkiRuleset().RulesetInfo));
    }

    [TestCaseSource(nameof(ObjectFlagsSource))]
    public void PerformNoteTest(bool breakState = false, bool ex = false)
    {
        AddStep("Miss Single", () => testSingle(false, breakState, ex));
        AddStep("Hit Single", () => testSingle(true, breakState, ex));
        AddUntilStep("Wait for object despawn", () => !Children.Any(h => h is DrawableSentakkiHitObject sentakkiHitObject && sentakkiHitObject.AllJudged == false));
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

        Add(new DrawableTap(circle)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Depth = depthIndex++,
            Auto = auto
        });
    }
}
