using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Rulesets.Sentakki.Skinning.Common;

namespace osu.Game.Rulesets.Sentakki.Tests.Graphics;

[TestFixture]
public partial class TestSceneFanChevronVisual : SentakkiSkinnableTestScene
{
    private readonly SlideBodyInfo slideBodyInfo = new SlideBodyInfo();

    [Cached]
    private readonly SlideChevronProvider chevronPool;

    public TestSceneFanChevronVisual()
    {
        AddInternal(chevronPool = new SlideChevronProvider());

    }
}
