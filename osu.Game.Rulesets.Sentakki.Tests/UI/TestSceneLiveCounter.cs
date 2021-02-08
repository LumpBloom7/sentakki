using NUnit.Framework;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.UI
{
    [TestFixture]
    public class TestSceneLiveCounter : OsuTestScene
    {
        private LiveCounter counter;

        public TestSceneLiveCounter()
        {
            AddStep("Clear test", () =>
            {
                Clear();
            });

            AddStep("Create Ring", () => Add(counter = new LiveCounter()));
            AddUntilStep("Ring loaded", () => counter.IsLoaded);
            AddStep("Lose a live", () => counter.LivesLeft.Value--);
        }
    }
}
