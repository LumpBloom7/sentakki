using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    // New Judgement type to completely avoid the problem of legacy skins, they aren't appropriate for custom rulesets that use varied HitResults
    public partial class DrawableSentakkiJudgement : PoolableDrawable
    {
        public override bool RemoveCompletedTransforms => false;

        private Container judgementBody = null!;
        private SentakkiJudgementPiece judgementPiece = null!;
        private OsuSpriteText timingPiece = null!;

        private readonly BindableBool detailedJudgements = new BindableBool();

        [Resolved]
        private DrawableSentakkiRuleset drawableRuleset { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(SentakkiRulesetConfigManager sentakkiConfigs)
        {
            sentakkiConfigs?.BindWith(SentakkiRulesetSettings.DetailedJudgements, detailedJudgements);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AddInternal(
                judgementBody = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(0.9f),
                    Children = new Drawable[]
                    {
                        timingPiece = new OsuSpriteText
                        {
                            Y = -15,
                            Origin = Anchor.Centre,
                            Font = OsuFont.Torus.With(size: 20, weight: FontWeight.Bold),
                            Shadow = true,
                            ShadowColour = Color4.Black
                        },
                        judgementPiece = new SentakkiJudgementPiece(HitResult.Great)
                    }
                }
            );
        }

        public DrawableSentakkiJudgement Apply(JudgementResult result, DrawableHitObject hitObject)
        {
            if (result.Type == HitResult.Perfect && detailedJudgements.Value)
                judgementPiece.JudgementText.Text = HitResult.Great.GetDisplayNameForSentakkiResult().ToUpperInvariant();
            else
                judgementPiece.JudgementText.Text = result.Type.GetDisplayNameForSentakkiResult().ToUpperInvariant();

            judgementPiece.JudgementText.Colour = result.Type.GetColorForSentakkiResult();

            if (result.HitObject.HitWindows is SentakkiEmptyHitWindows || result.Type == HitResult.Miss || !detailedJudgements.Value)
            {
                timingPiece.Alpha = 0;
            }
            else
            {
                timingPiece.Alpha = 1;

                if (result.Type == HitResult.Perfect)
                {
                    timingPiece.Text = "CRITICAL";
                    timingPiece.Colour = result.Type.GetColorForSentakkiResult();
                }
                else if (result.TimeOffset > 0)
                {
                    timingPiece.Text = "LATE";
                    timingPiece.Colour = Color4.OrangeRed;
                }
                else if (result.TimeOffset < 0)
                {
                    timingPiece.Text = "EARLY";
                    timingPiece.Colour = Color4.GreenYellow;
                }
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

            return this;
        }

        protected override void PrepareForUse()
        {
            ApplyTransformsAt(double.MinValue, true);
            ClearTransforms(true);

            applyHitAnimations();
        }

        private void applyHitAnimations()
        {
            double speedFactor = drawableRuleset.GameplaySpeed;
            judgementBody.ScaleTo(1, 50 * speedFactor, Easing.OutElastic);

            judgementBody.Delay(50 * speedFactor)
                         .ScaleTo(0.8f, 300 * speedFactor)
                         .FadeOut(300 * speedFactor);

            this.Delay(350 * speedFactor).Expire();
        }

        private partial class SentakkiJudgementPiece : DefaultJudgementPiece
        {
            public SentakkiJudgementPiece(HitResult result)
                : base(result)
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
