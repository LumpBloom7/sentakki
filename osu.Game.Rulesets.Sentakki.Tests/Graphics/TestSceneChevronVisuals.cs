using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.Skinning;
using osu.Game.Rulesets.Sentakki.Skinning.Default;
using osu.Game.Rulesets.Sentakki.Skinning.Default.Slide;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Tests.Graphics;

[TestFixture]
public partial class TestSceneChevronVisual : SentakkiSkinnableTestScene
{

    public TestSceneChevronVisual()
    {
        SetContents(_ => new SkinnableDrawable(new SentakkiSkinComponentLookup(SentakkiSkinComponents.SlideChevron), _ => new SlideChevron()));
    }
}
