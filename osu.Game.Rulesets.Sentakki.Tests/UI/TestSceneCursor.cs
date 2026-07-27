using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Skinning;
using osu.Game.Rulesets.Sentakki.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Tests.UI;

[TestFixture]
public partial class TestSceneCursor : SentakkiSkinnableTestScene
{
    [Test]
    public void TestCursor()
    {
        Schedule(() => SetContents(_ => new SkinnableDrawable(new SentakkiSkinComponentLookup(SentakkiSkinComponents.Cursor), _ => new SentakkiCursor())
        {
            RelativeSizeAxes = Axes.None
        }));
    }
}
