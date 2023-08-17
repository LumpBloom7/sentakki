using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Touches
{
    public partial class TouchGlowPiece : CompositeDrawable
    {
        private Texture touchTexture = null!;


        private Bindable<bool> ExNoteBindable = new Bindable<bool>(true);

        public TouchGlowPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, DrawableHitObject hitObject)
        {
            touchTexture = textures.Get("TouchGlow");
            AddInternal(new Sprite
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Y = -34, // HACK
                Texture = touchTexture,
            });

            // Bind exnote
            ExNoteBindable.BindTo(((DrawableSentakkiHitObject)hitObject).ExModifierBindable);
            ExNoteBindable.BindValueChanged(v => Colour = v.NewValue ? Color4.White : Color4.Black, true);
        }


    }
}
