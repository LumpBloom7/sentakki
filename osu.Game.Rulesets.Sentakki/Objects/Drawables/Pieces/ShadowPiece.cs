using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public partial class ShadowPiece : Container
    {
        private CircularContainer glowContainer;

        private Bindable<bool> ExBindable = new Bindable<bool>(true);
        private Bindable<Color4> AccentColour = new Bindable<Color4>();

        private static readonly EdgeEffectParameters shadow_parameters = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Radius = 15,
            Colour = Color4.Black,
            Hollow = true
        };

        public ShadowPiece()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Padding = new MarginPadding(1);

            Child = glowContainer = new CircularContainer
            {
                Alpha = .5f,
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                EdgeEffect = shadow_parameters
            };
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject hitObject)
        {
            // Bind exnote
            ExBindable.BindTo(((DrawableSentakkiHitObject)hitObject).ExBindable);
            AccentColour.BindTo(hitObject.AccentColour);

            AccentColour.BindValueChanged(_ => updateGlow());
            ExBindable.BindValueChanged(_ => updateGlow(), true);
        }

        private void updateGlow()
        {
            if (!ExBindable.Value)
            {
                glowContainer.EdgeEffect = shadow_parameters;
                return;
            }

            glowContainer.EdgeEffect = shadow_parameters with
            {
                Colour = AccentColour.Value
            };
        }
    }
}
