using NUnit.Framework;
using osu.Game.Rulesets.Sentakki.Mods;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.Mods;

public partial class TestSceneModSynesthesia : ModTestScene
{
    protected override Ruleset CreatePlayerRuleset() => new SentakkiRuleset();

    [Test]
    public void TestDivisorMode() => CreateModTest(new ModTestData
    {
        Mod = new SentakkiModSynesthesia()
        {
            IntervalColouring = { Value = false }
        },
        Autoplay = true,
        PassCondition = () => true
    });

    [Test]
    public void TestIntervalMode() => CreateModTest(new ModTestData
    {
        Mod = new SentakkiModSynesthesia()
        {
            IntervalColouring = { Value = true }
        },
        Autoplay = true,
        PassCondition = () => true
    });
}
