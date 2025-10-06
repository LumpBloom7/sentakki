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
public partial class TestSceneTouchNote : OsuTestScene
{
    private readonly Container content;
    protected override Container<Drawable> Content => content;

    private int depthIndex;

    public TestSceneTouchNote()
    {
        base.Content.Add(content = new SentakkiInputManager(new SentakkiRuleset().RulesetInfo));
        base.Content.Add(new SentakkiRing
        {
            RelativeSizeAxes = Axes.None,
            Size = new Vector2(SentakkiPlayfield.RINGSIZE)
        });
    }

    [TestCaseSource(nameof(ObjectFlagsSource))]
    public void TestTouchNotes(bool breakState, bool ex)
    {
        AddStep("Miss Single", () => testAllPositions(false, breakState, ex));
        AddStep("Hit Single", () => testAllPositions(true, breakState, ex));
        AddUntilStep("Wait for object despawn", () => !Children.Any(h => h is DrawableSentakkiHitObject sentakkiHitObject && sentakkiHitObject.AllJudged == false));
    }

    public static bool[][] ObjectFlagsSource =
    [
        [false, false],
        [true, false],
        [false, true],
        [true, true],
    ];

    private void testAllPositions(bool auto = false, bool breakState = false, bool ex = false)
    {
        foreach (var position in SentakkiBeatmapConverterOld.VALID_TOUCH_POSITIONS)
        {
            var circle = new Touch
            {
                StartTime = Time.Current + 1000,
                Position = position,
                Break = breakState,
                Ex = ex
            };

            if (breakState)
                circle.NoteColour = Color4.OrangeRed;

            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            Add(new DrawableTouch(circle)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Depth = depthIndex++,
                Auto = auto
            });
        }
    }

    protected override Ruleset CreateRuleset() => new SentakkiRuleset();
}
