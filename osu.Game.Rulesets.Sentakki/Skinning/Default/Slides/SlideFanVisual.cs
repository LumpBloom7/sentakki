using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default.Slides
{
    public class SlideFanVisual : SlideVisualBase<SlideFanVisual.SlideFanChevronView>
    {
        public SlideFanVisual()
        {
            Rotation = 22.5f;
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(SlideFanChevrons fanChevrons)
        {
            const double endpoint_distance = 80; // margin for each end

            for (int i = 10; i >= 0; --i)
            {
                float progress = (i + 2) / (float)12;
                float scale = progress;
                Chevrons.Add(new SlideFanChevronView(fanChevrons.Get(i))
                {
                    Y = ((SentakkiPlayfield.RINGSIZE + 50 - (float)endpoint_distance) * scale) - 350,
                    Progress = (i + 1) / (float)11,
                });
            }
        }

        public class SlideFanChevronView : CompositeDrawable, ISlideChevron
        {
            public double Progress { get; set; }
            private readonly IBindable<Vector2> textureSize = new Bindable<Vector2>();

            public SlideFanChevronView((BufferedContainerView<Drawable> view, IBindable<Vector2> sizeBindable) chevron)
            {
                Anchor = Origin = Anchor.Centre;

                textureSize.BindValueChanged(v => Size = v.NewValue);
                textureSize.BindTo(chevron.sizeBindable);

                AddInternal(chevron.view);
            }
        }
    }
}
