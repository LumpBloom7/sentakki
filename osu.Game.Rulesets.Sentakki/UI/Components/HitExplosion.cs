using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI.Components
{
    public partial class HitExplosion : PoolableDrawable
    {
        public override bool RemoveWhenNotAlive => true;

        private readonly CircularContainer circle;

        private const float default_explosion_size = 75;
        private const float touch_hold_explosion_size = 100;

        [Resolved]
        private DrawableSentakkiRuleset? drawableRuleset { get; set; }

        public HitExplosion()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(default_explosion_size);
            Colour = Color4.Cyan;
            Alpha = 0;
            InternalChildren = new Drawable[]
            {
                circle = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 45,
                    BorderColour = Color4.White,
                    Child = new Box
                    {
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both,
                        AlwaysPresent = true,
                    }
                },
            };

            borderRatio.BindValueChanged(setBorderThiccness, true);
        }

        private void setBorderThiccness(ValueChangedEvent<float> v)
        {
            circle.BorderThickness = Size.X / 2 * v.NewValue;
        }

        private readonly BindableFloat borderRatio = new BindableFloat(1);

        public HitExplosion Apply(DrawableSentakkiHitObject drawableSentakkiHitObject)
        {
            Colour = drawableSentakkiHitObject.AccentColour.Value;

            switch (drawableSentakkiHitObject.HitObject)
            {
                case SentakkiLanedHitObject lanedObject:
                    Position = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, lanedObject.Lane);
                    Size = new Vector2(default_explosion_size);
                    break;

                case Touch touchObject:
                    Position = touchObject.Position;
                    Size = new Vector2(default_explosion_size);
                    break;

                case TouchHold _:
                default:
                    Position = Vector2.Zero;
                    Size = new Vector2(touch_hold_explosion_size);
                    break;
            }

            return this;
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            // When used as a standalone piece in TOUCH and TOUCHHOLDs, we want to avoid the animation playing out immediately
            if (!IsInPool)
                return;

            Explode().Expire(true);
        }

        public TransformSequence<HitExplosion> Explode()
        {
            const double explode_duration = 100;

            double adjustedExplodeDuration = explode_duration * (drawableRuleset?.GameplaySpeed ?? 1);

            var sequence = this.FadeTo(0.8f)
                               .TransformBindableTo(borderRatio, 1)
                               .ScaleTo(1)
                               .Then()
                               .TransformBindableTo(borderRatio, 0f, adjustedExplodeDuration)
                               .ScaleTo(2f, adjustedExplodeDuration)
                               .FadeOut(adjustedExplodeDuration);

            return sequence;
        }
    }
}
