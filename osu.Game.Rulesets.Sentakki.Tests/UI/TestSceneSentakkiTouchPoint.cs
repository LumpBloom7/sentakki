using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Timing;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Tests.UI
{
    [TestFixture]
    public class TestSceneSentakkiTouchPoint : OsuGridTestScene
    {
        public TestSceneSentakkiTouchPoint() : base(1, 2)
        {
            for (int i = 0; i < 2; ++i)
            {
                TouchVisualization.TouchPointer tmp;
                int j = i;
                Cell(i).Add(tmp = new TouchVisualization.TouchPointer().With(d => d.Scale = new Vector2(i == 0 ? -1 : 1, 1)));
                tmp.Show();
            }
        }
    }
}
