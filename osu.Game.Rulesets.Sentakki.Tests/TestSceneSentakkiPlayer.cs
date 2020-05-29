using NUnit.Framework;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests
{
    [TestFixture]
    public class TestSceneSentakkiPlayer : PlayerTestScene
    {
        public TestSceneSentakkiPlayer()
            : base(new SentakkiRuleset())
        {
        }
    }
}
