using NUnit.Framework;
using osu.Game.Rulesets.Sentakki.UI;

namespace osu.Game.Rulesets.Sentakki.Tests.UI;

[TestFixture]
public partial class TestSceneCursor : SentakkiSkinnableTestScene
{
    [Test]
    public void TestCursor()
    {
        SetContents(_ => new SentakkiCursorContainer());
    }
}
