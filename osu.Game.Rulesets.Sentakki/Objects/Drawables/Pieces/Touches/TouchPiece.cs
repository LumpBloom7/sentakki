using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Touches
{
    public partial class TouchPiece : CompositeDrawable
    {
        private Texture touchTexture = null!;

        public TouchPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            //RelativePositionAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            touchTexture = textures.Get("TouchNoGlow");
            AddInternal(new Sprite
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Y = -34, // HACK
                Texture = touchTexture,
            });
        }
    }
}
