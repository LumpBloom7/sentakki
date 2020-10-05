using NUnit.Framework;
using osu.Framework.Input;
using osu.Framework.Timing;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.UI
{
    [TestFixture]
    public class TestSceneSentakkiTouchPoint : OsuGridTestScene
    {
        public TestSceneSentakkiTouchPoint() : base(5, 2)
        {
            for (int i = 0; i < 10; ++i)
            {
                TouchVisualization.TouchPointer tmp;
                Cell(i).Add(tmp = new TouchVisualization.TouchPointer((TouchSource)i));
                tmp.Show();
            }
        }
    }
}
