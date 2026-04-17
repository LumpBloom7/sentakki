using NUnit.Framework;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.UI;

[TestFixture]
public partial class TestSceneResumeOverlay : OsuTestScene
{
    protected override Ruleset CreateRuleset() => new SentakkiRuleset();

    public TestSceneResumeOverlay()
    {
        SentakkiCursorContainer gameplayCursor;
        Add(gameplayCursor = new SentakkiCursorContainer());

        SentakkiResumeOverlay resumeOverlay;

        Add(resumeOverlay = new SentakkiResumeOverlay()
        {
            GameplayCursor = gameplayCursor
        });

        AddWaitStep("Wait", 5);
        AddStep("Show resume overlay", () => resumeOverlay.Show());
        AddWaitStep("Wait", 5);
    }
}
