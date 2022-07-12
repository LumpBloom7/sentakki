using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides
{
    public class StarPiece : CompositeDrawable
    {
        public StarPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            AddInternal(new Sprite()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Texture = textures.Get("star"),
            });
        }
    }
}
