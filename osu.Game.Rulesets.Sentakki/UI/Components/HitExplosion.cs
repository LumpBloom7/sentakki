using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI.Components
{
    public class HitExplosion : CircularContainer
    {
        public HitExplosion()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(75);
            Colour = Color4.Cyan;
            Masking = true;
            BorderThickness = 45;
            BorderColour = Color4.White;
            Alpha = 0;
            InternalChildren = new Drawable[]{
                new Box
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    AlwaysPresent = true
                },
            };
            borderRatio.BindValueChanged(_ => setBorderThiccness(), true);
        }
        private void setBorderThiccness()
        {
            BorderThickness = Size.X / 2 * borderRatio.Value;
        }

        private BindableFloat borderRatio = new BindableFloat(1);

        public void Animate()
        {
            FinishTransforms(true);
            this.FadeIn().TransformBindableTo(borderRatio, 1).ScaleTo(1).Then().TransformBindableTo(borderRatio, 0f, 100).ScaleTo(1.5f, 100);
        }
    }
}
