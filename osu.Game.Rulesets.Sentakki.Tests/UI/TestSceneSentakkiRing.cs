using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Skinning;
using osu.Game.Rulesets.Sentakki.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Tests.UI;

[TestFixture]
public partial class TestSceneSentakkiRing : SentakkiSkinnableTestScene
{
    [Test]
    public void DisplayRing()
    {
        Schedule(() => SetContents(_ => new SkinnableDrawable(new SentakkiSkinComponentLookup(SentakkiSkinComponents.PlayfieldRing), _ => new PlayfieldRing())
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Scale = new Vector2(0.3f),
        }));
    }
}
