using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Sentakki.Skinning.Default.Slide;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Skinning.Common;

public partial class SlideChevronProvider : CompositeDrawable
{
    private ChevronPool chevronPool = null!;
    private Dictionary<int, FanChevronPool> fanChevronPools = [];

    public SlideChevronProvider()
    {
        AddInternal(chevronPool = new ChevronPool(64));

        for (int i = 0; i < 11; ++i)
            AddInternal(fanChevronPools[i] = new FanChevronPool(i));
    }

    public PoolableGameplayChevron GetChevron() => chevronPool.Get();
    public PoolableGameplayChevron GetFanChevron(int index) => fanChevronPools[index].Get();

    private partial class ChevronPool(int initialSize, int? maximumSize = null) : DrawablePool<PoolableGameplayChevron>(initialSize, maximumSize)
    {
        protected override PoolableGameplayChevron CreateNewDrawable()
            => new PoolableGameplayChevron(
                new SkinnableDrawable(
                    new SentakkiSkinComponentLookup(SentakkiSkinComponents.SlideChevron),
                    _ => new SlideChevron()
                ));
    }

    private partial class FanChevronPool(int index) : DrawablePool<PoolableGameplayChevron>(1)
    {
        protected override PoolableGameplayChevron CreateNewDrawable()
            => new PoolableGameplayChevron(
                new SkinnableDrawable(
                    new SentakkiSkinComponentLookup(SentakkiSkinComponents.SlideFanChevron0 + index),
                    _ => new SlideFanChevron(index)
                ));
    }
}
