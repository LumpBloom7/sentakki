using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides
{
    public partial class StarPiece : CompositeDrawable
    {
        private Sprite glowTexture = null!;

        private Bindable<bool> ExNoteBindable = new Bindable<bool>();

        public StarPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            AddInternal(glowTexture = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Texture = textures.Get("starGlow"),
                Colour = Color4.White
            });

            AddInternal(new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Texture = textures.Get("starNoGlow"),
            });
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject hitObject)
        {
            // Bind exnote
            ExNoteBindable.BindValueChanged(v => Colour = v.NewValue ? Color4.White : Color4.Black);
        }
    }
}
