using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default.Slides
{
    public class SlideChevron : PoolableDrawable, ISlideChevron
    {
        public double Progress { get; set; }

        public SlideChevron()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            AddInternal(new SkinnableDrawable(new SentakkiSkinComponent(SentakkiSkinComponents.SlideChevron), _ => new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Texture = textures.Get("slide"),
            }));
        }

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();
            ClearTransforms();
        }
    }
}
