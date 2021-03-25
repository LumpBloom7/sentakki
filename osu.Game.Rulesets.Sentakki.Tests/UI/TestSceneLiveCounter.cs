using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.UI
{
    [TestFixture]
    public class TestSceneLiveCounter : OsuTestScene
    {
        private LiveCounter counter;
        private BindableInt lives = new BindableInt
        {
            MaxValue = 500,
            Value = 500
        };

        public TestSceneLiveCounter()
        {
            AddStep("Clear test", () =>
            {
                Clear();
            });

            AddStep("Create counter", () => Add(counter = new LiveCounter(lives)));
            AddUntilStep("Counter loaded", () => counter.IsLoaded);
            AddStep("Lose a live", () => counter.LivesLeft.Value -= 1);
            AddStep("Lose two lives", () => counter.LivesLeft.Value -= 2);
            AddStep("Lose five lives", () => counter.LivesLeft.Value -= 5);
        }
    }
}
