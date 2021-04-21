using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI.Components
{
    public class HitExplosion : PoolableDrawable
    {
        private readonly CircularContainer circle;

        public HitExplosion()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(75);
            Colour = Color4.Cyan;
            Alpha = 0;
            InternalChildren = new Drawable[]{
                circle = new CircularContainer{
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 45,
                    BorderColour = Color4.White,
                    Child = new Box{
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both,
                        AlwaysPresent = true,
                    }
                },
            };
            borderRatio.BindValueChanged(_ => setBorderThiccness(), true);
        }
        private void setBorderThiccness()
        {
            circle.BorderThickness = Size.X / 2 * borderRatio.Value;
        }

        private readonly BindableFloat borderRatio = new BindableFloat(1);

        protected override void PrepareForUse()
        {
            base.PrepareForUse();
            if (IsInPool)
            {
                Animate();
                Expire();
            }
        }

        public void Animate()
        {
            FinishTransforms(true);
            this.FadeIn().TransformBindableTo(borderRatio, 1).ScaleTo(1).Then().TransformBindableTo(borderRatio, 0f, 100).ScaleTo(2f, 100).FadeOut(150);
        }
    }
}
