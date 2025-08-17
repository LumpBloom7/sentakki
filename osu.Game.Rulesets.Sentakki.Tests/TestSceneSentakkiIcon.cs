using NUnit.Framework;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests;

[TestFixture]
public partial class TestSceneSentakkiIcon : OsuTestScene
{
    protected override Ruleset CreateRuleset() => new SentakkiRuleset();

    protected override void LoadComplete()
    {
        base.LoadComplete();
        Add(new SentakkiRuleset.SentakkiIcon(CreateRuleset()));
    }
}
