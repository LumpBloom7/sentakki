using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Layout;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default.Slides
{
    /// <summary>
    /// This drawable holds a set of all chevron buffered drawables, and is used to preload all/draw of them outside of playfield. (To avoid Playfield transforms re-rendering the chevrons)
    /// <br/>
    /// A view of each chevron, along with their size, would be used by SlideFanVisual.
    /// </summary>
    public class SlideFanChevrons : CompositeDrawable
    {
        private Container<ChevronBackingTexture> chevrons;

        public SlideFanChevrons()
        {
            Alpha = 0;
            AlwaysPresent = true;

            // we are doing this in ctor to guarantee that this object is properly initialized before BDL
            loadChevronsTextures();
        }

        public (BufferedContainerView<Drawable>, IBindable<Vector2>) Get(int index)
        {
            var chevron = chevrons[index];

            var view = chevron.CreateView();
            view.RelativeSizeAxes = Axes.Both;

            return (view, chevron.SizeBindable);
        }

        private void loadChevronsTextures()
        {
            AddInternal(chevrons = new Container<ChevronBackingTexture>());

            for (int i = 0; i < 11; ++i)
            {
                float progress = (i + 2) / (float)12;
                float scale = progress;

                chevrons.Add(new ChevronBackingTexture(scale));
            }
        }

        private class ChevronBackingTexture : BufferedContainer
        {
            public Bindable<Vector2> SizeBindable { get; } = new Bindable<Vector2>();

            // This is to ensure that drawables using this texture is sized correctly (since autosize only happens during the first update)
            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                if (invalidation == Invalidation.DrawSize)
                    SizeBindable.Value = DrawSize;

                return base.OnInvalidate(invalidation, source);
            }

            public ChevronBackingTexture(float progress) : base(cachedFrameBuffer: true)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                AutoSizeAxes = Axes.Both;

                AddInternal(new SkinnableDrawable(new SentakkiSkinComponent(SentakkiSkinComponents.SlideFanChevron), _ => new SlideFanChevron(progress))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    AutoSizeAxes = Axes.Both,
                });
            }
        }
    }
}
