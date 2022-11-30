using NUnit.Framework;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests
{
    [TestFixture]
    public partial class TestSceneSentakkiPlayer : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new SentakkiRuleset();
        protected override bool Autoplay => true;
    }
}
