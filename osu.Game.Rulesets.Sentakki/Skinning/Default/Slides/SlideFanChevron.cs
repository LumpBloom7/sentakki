using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default.Slides
{
    public class SlideFanChevron : CompositeDrawable, ISlideChevron
    {
        public double Progress { get; set; }
        private readonly IBindable<Vector2> textureSize = new Bindable<Vector2>();

        public SlideFanChevron((BufferedContainerView<Drawable> view, IBindable<Vector2> sizeBindable) chevron)
        {
            Anchor = Origin = Anchor.Centre;

            textureSize.BindValueChanged(v => Size = v.NewValue);
            textureSize.BindTo(chevron.sizeBindable);

            AddInternal(chevron.view);
        }
    }
}
