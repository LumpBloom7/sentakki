using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Bindables;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class HitObjectLine : CompositeDrawable
    {
        public HitObjectLine()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = new Vector2(.22f);
            Size = new Vector2(299);
            Alpha = 0;
        }

        private readonly IBindable<ArmedState> state = new Bindable<ArmedState>();
        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject, TextureStore textures)
        {
            state.BindTo(drawableObject.State);
            state.BindValueChanged(updateState, true);

            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour => Colour = colour.NewValue, true);

            AddInternal(new Sprite()
            {
                RelativeSizeAxes = Axes.Both,
                Rotation = -45,
                Anchor = Anchor.Centre,
                Origin = Anchor.BottomLeft,
                Texture = textures.Get("HitObjectLine")
            });
        }

        private void updateState(ValueChangedEvent<ArmedState> state)
        {
            switch (state.NewValue)
            {
                case ArmedState.Miss:
                case ArmedState.Hit:
                    this.FadeOut();
                    break;
            }
        }
    }
}
