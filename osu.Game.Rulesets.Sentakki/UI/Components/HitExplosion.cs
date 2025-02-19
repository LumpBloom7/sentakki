using System;
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

        private readonly HitExplosionContainer visual;

        private const float default_explosion_size = 75;
        private const float touch_hold_explosion_size = 100;

        public HitExplosion()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(default_explosion_size);
            Colour = Color4.White;
            Alpha = 0;
            InternalChildren =
            [
                visual = new HitExplosionContainer()
            ];

            borderRatio.BindValueChanged(setBorderThiccness, true);
        }

        private void setBorderThiccness(ValueChangedEvent<float> v)
        {
            visual.BorderThickness = Size.X / 2 * v.NewValue;
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
                    visual.Circular = true;
                    break;

                case Touch touchObject:
                    Position = touchObject.Position;
                    Size = new Vector2(default_explosion_size);
                    visual.Circular = false;
                    Rotation = 0;
                    break;

                case TouchHold _:
                default:
                    Position = Vector2.Zero;
                    Size = new Vector2(touch_hold_explosion_size);
                    visual.Circular = false;
                    Rotation = 45;
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

        public TransformSequence<HitExplosion> Explode(double explode_duration = 80)
        {
            var sequence = this.FadeTo(0.8f)
                               .TransformBindableTo(borderRatio, 1)
                               .ScaleTo(1)
                               .Then()
                               .TransformBindableTo(borderRatio, 0f, duration: explode_duration)
                               .ScaleTo(2f, explode_duration)
                               .FadeOut(explode_duration);

            return sequence;
        }

        private partial class HitExplosionContainer : Container
        {
            public bool Circular { get; set; } = true;

            public HitExplosionContainer()
            {
                Masking = true;
                CornerExponent = 2;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                RelativeSizeAxes = Axes.Both;
                BorderColour = Color4.White;
                Child = new Box
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    AlwaysPresent = true,
                };
            }

            protected override void Update()
            {
                base.Update();
                if (Circular)
                    CornerRadius = Math.Min(DrawSize.X, DrawSize.Y) * 0.5f;
                else
                    CornerRadius = 20;
            }
        }
    }
}
