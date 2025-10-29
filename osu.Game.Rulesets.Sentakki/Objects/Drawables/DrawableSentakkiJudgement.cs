using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables;

// New Judgement type to completely avoid the problem of legacy skins, they aren't appropriate for custom rulesets that use varied HitResults
public partial class DrawableSentakkiJudgement : PoolableDrawable
{
    public override bool RemoveCompletedTransforms => false;

    private Container judgementBody = null!;
    private SentakkiJudgementPiece judgementPiece = null!;
    private OsuSpriteText timingPiece = null!;

    private readonly BindableBool detailedJudgements = new BindableBool();

    [Resolved]
    private OsuColour colours { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load(SentakkiRulesetConfigManager? sentakkiConfigs)
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
                Children =
                [
                    timingPiece = new OsuSpriteText
                    {
                        Y = -15,
                        Origin = Anchor.Centre,
                        Font = OsuFont.Torus.With(size: 20, weight: FontWeight.Bold),
                        Shadow = true,
                        ShadowColour = Color4.Black
                    },
                    judgementPiece = new SentakkiJudgementPiece(HitResult.Great)
                ]
            }
        );
    }

    public DrawableSentakkiJudgement Apply(JudgementResult result, DrawableHitObject hitObject)
    {
        if (result.Type == HitResult.Miss || !detailedJudgements.Value)
            timingPiece.Alpha = 0;
        // While TouchHolds don't have hit windows, we display the timing text anyways when a crit is obtained
        else if (result.HitObject is TouchHold && result.Type is HitResult.Perfect)
            timingPiece.Alpha = 1;
        else if (result.HitObject.HitWindows is SentakkiEmptyHitWindows)
            timingPiece.Alpha = 0;
        else
            timingPiece.Alpha = 1;

        judgementPiece.JudgementText.Colour = colours.ForSentakkiResult(result.Type);

        if (result.Type == HitResult.Perfect)
        {
            timingPiece.Text = "CRITICAL";
            timingPiece.Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#00FFAA"), Color4Extensions.FromHex("7CF6FF"));
            judgementPiece.JudgementText.Text =
                (detailedJudgements.Value ? HitResult.Great : result.Type).GetDisplayNameForSentakkiResult().ToUpperInvariant();
        }
        else
        {
            judgementPiece.JudgementText.Text =
                result.Type.GetDisplayNameForSentakkiResult().ToUpperInvariant();

            switch (result.TimeOffset)
            {
                case > 0:
                    timingPiece.Text = "LATE";
                    timingPiece.Colour = Color4.OrangeRed;
                    break;

                case < 0:
                    timingPiece.Text = "EARLY";
                    timingPiece.Colour = Color4.GreenYellow;
                    break;
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
        judgementBody.ScaleTo(1, 50, Easing.OutElastic);

        judgementBody.Delay(50)
                     .ScaleTo(0.8f, 300)
                     .FadeOut(300);

        this.Delay(350).Expire();
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
