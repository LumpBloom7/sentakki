using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Game.Rulesets.Sentakki.Objects;
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

        public void Apply(SentakkiLanedHitObject lanedHitObject)
        {
            Position = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, lanedHitObject.Lane);
            Colour = lanedHitObject.NoteColour;
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            // When used as a standalone piece in TOUCH and TOUCHHOLDs, we want to avoid the animation playing out immediately
            if (!IsInPool)
                return;

            Explode().Expire();
        }

        public TransformSequence<HitExplosion> Explode()
        {
            const double explode_duration = 100;

            var sequence = this.FadeIn()
                               .TransformBindableTo(borderRatio, 1)
                               .ScaleTo(1)
                               .Then()
                               .TransformBindableTo(borderRatio, 0f, explode_duration)
                               .ScaleTo(2f, explode_duration)
                               .FadeOut(explode_duration);

            return sequence;
        }
    }
}
