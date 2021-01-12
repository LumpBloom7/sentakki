using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    // New Judgement type to completely avoid the problem of legacy skins, they aren't appropriate for custom rulesets that use varied HitResults
    public class DrawableSentakkiJudgement : PoolableDrawable
    {
        [Resolved]
        private OsuColour colours { get; set; }

        public override bool RemoveCompletedTransforms => false;


        private Container judgementBody;
        private SentakkiJudgementPiece judgementPiece;
        private OsuSpriteText timingPiece;

        private HitResult result = HitResult.Great;

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AddInternal(
                judgementBody = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(0.9f),
                    Children = new Drawable[]{
                        timingPiece = new OsuSpriteText{
                            Y = -25,
                            Origin = Anchor.Centre,
                            Font = OsuFont.Torus.With(size: 25, weight: FontWeight.SemiBold),
                            Shadow = true,
                            ShadowColour = Color4.Black
                        },
                        judgementPiece = new SentakkiJudgementPiece(result)
                    }
                }
            );
        }

        public void Apply(JudgementResult result, DrawableHitObject hitObject)
        {
            this.result = result.Type;
            judgementPiece.JudgementText.Text = result.Type.GetDescription().ToUpperInvariant();
            judgementPiece.JudgementText.Colour = result.Type.GetColorForSentakkiResult();

            if (Math.Abs(result.TimeOffset) > 20 && result.Type != HitResult.Miss)
            {
                timingPiece.Alpha = 1;
                if (result.TimeOffset > 0)
                {
                    timingPiece.Text = "Late";
                    timingPiece.Colour = Color4.OrangeRed;
                }
                else
                {
                    timingPiece.Text = "Early";
                    timingPiece.Colour = Color4.GreenYellow;
                }
            }
            else
            {
                timingPiece.Alpha = 0;
            }

            LifetimeStart = result.TimeAbsolute;

            switch (hitObject)
            {
                case DrawableSentakkiLanedHitObject laned:
                    Position = SentakkiExtensions.GetPositionAlongLane(240, laned.HitObject.Lane);
                    Rotation = laned.HitObject.Lane.GetRotationForLane();
                    break;
                default:
                    Position = hitObject.Position;
                    Rotation = 0;
                    break;
            }
        }

        protected override void PrepareForUse()
        {
            ApplyTransformsAt(double.MinValue, true);
            ClearTransforms(true);

            applyHitAnimations();
        }

        private void applyHitAnimations()
        {
            judgementBody.ScaleTo(1, 50, Easing.OutElastic);

            judgementBody.Delay(50)
                         .ScaleTo(0.8f, 300)
                         .FadeOut(300);

            this.Delay(350).Expire();
        }

        private class SentakkiJudgementPiece : DefaultJudgementPiece
        {
            public SentakkiJudgementPiece(HitResult result) : base(result)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                JudgementText.Scale = Vector2.One;
                JudgementText.Font = OsuFont.Torus.With(size: 30, weight: FontWeight.Bold);
                JudgementText.Shadow = true;
                JudgementText.ShadowColour = Color4.Black;
            }

            public new SpriteText JudgementText => base.JudgementText;
        }
    }
}
