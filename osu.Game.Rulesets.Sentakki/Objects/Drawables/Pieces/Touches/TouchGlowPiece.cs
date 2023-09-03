using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Touches
{
    public partial class TouchGlowPiece : CompositeDrawable
    {
        private Bindable<bool> ExBindable = new Bindable<bool>();

        public TouchGlowPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, DrawableHitObject? hitObject)
        {
            var tex = textures.Get("touchGlow");

            // We must shift the texture down by this amount to align the sprite to the top edge of the triangle
            const float triangle_half_height = 43f;

            // The textures are potentially scaled down when in use, so let's account for that (osu does supersampling by default)
            float yShift = triangle_half_height / tex.ScaleAdjust;

            AddInternal(new Sprite
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.Centre,
                Y = yShift,
                Texture = tex,
            });

            if (hitObject is null)
                return;

            // Bind exnote
            ExBindable.BindTo(((DrawableSentakkiHitObject)hitObject).ExBindable);
            ExBindable.BindValueChanged(v => Colour = v.NewValue ? Color4.White : Color4.Black, true);
        }
    }
}
