using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class GlowPiece : Container
    {
        public GlowPiece()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Padding = new MarginPadding(1);
        }

        protected override void LoadComplete()
        {
            Child = new CircularContainer
            {
                Alpha = .5f,
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                EdgeEffect = new EdgeEffectParameters
                {
                    Hollow = true,
                    Type = EdgeEffectType.Glow,
                    Radius = 15,
                    Colour = Colour,
                }
            };
        }
    }
}
