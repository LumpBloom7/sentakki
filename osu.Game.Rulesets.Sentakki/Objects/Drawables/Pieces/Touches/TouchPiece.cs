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
            Size = new Vector2(75);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativePositionAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            touchTexture = textures.Get("Touch");
            AddInternal(new Sprite()
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                FillAspectRatio = 1,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Texture = touchTexture,
            });
        }
    }
}
