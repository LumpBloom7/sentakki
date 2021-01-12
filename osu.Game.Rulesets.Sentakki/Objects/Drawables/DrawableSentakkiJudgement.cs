using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
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

        private SentakkiJudgementPiece judgementBody;

        private HitResult result = HitResult.Great;

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AddInternal(judgementBody = new SentakkiJudgementPiece(result));
        }

        public void Apply(JudgementResult result, DrawableHitObject hitObject)
        {
            this.result = result.Type;
            judgementBody.JudgementText.Text = result.Type.GetDescription().ToUpperInvariant();
            judgementBody.JudgementText.Colour = result.Type.GetColorForSentakkiResult();

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
                Scale = new Vector2(.9f);
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
