using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default
{
    public class ShadowPiece : Container
    {
        public ShadowPiece()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Padding = new MarginPadding(1);

            Child = new CircularContainer
            {
                Alpha = .5f,
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                EdgeEffect = new EdgeEffectParameters
                {
                    Hollow = true,
                    Type = EdgeEffectType.Shadow,
                    Radius = 15,
                    Colour = Color4.Black,
                }
            };
        }
    }
}
