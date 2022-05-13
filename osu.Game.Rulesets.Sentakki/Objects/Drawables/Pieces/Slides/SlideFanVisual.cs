using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides
{
    public class SlideFanVisual : SlideVisualBase<SlideFanVisual.SlideFanChevron>
    {
        public SlideFanVisual()
        {
            Rotation = 22.5f;
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(SlideFanChevrons chevronTextures)
        {
            const double endpoint_distance = 80; // margin for each end

            for (int i = 10; i >= 0; --i)
            {
                float progress = (i + 2) / (float)12;
                float scale = progress;
                Chevrons.Add(new SlideFanChevron(chevronTextures.Get(i))
                {
                    Y = ((SentakkiPlayfield.RINGSIZE + 50 - (float)endpoint_distance) * scale) - 350,
                    Progress = (i + 1) / (float)11,
                });
            }
        }

        public class SlideFanChevron : CompositeDrawable, ISlideChevron
        {
            public double Progress { get; set; }
            private readonly IBindable<Vector2> textureSize = new Bindable<Vector2>();

            public SlideFanChevron(SlideFanChevrons.ChevronBackingTexture texture)
            {
                Anchor = Origin = Anchor.Centre;

                textureSize.BindValueChanged(v => Size = v.NewValue);
                textureSize.BindTo(texture.SizeBindable);

                AddInternal(texture.CreateView().With(d =>
                {
                    d.RelativeSizeAxes = Axes.Both;
                }));
            }
        }
    }
}
