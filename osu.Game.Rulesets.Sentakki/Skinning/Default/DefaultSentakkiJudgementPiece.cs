using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default;

public partial class DefaultSentakkiJudgementPiece : DefaultJudgementPiece, IHasTimingIndicator, IAnimatableJudgement
{
    [Resolved]
    private OsuColour colours { get; set; } = null!;

    private SpriteText? timingIndicatorText;

    Drawable? IAnimatableJudgement.GetAboveHitObjectsProxiedContent() => CreateProxy();

    public DefaultSentakkiJudgementPiece(HitResult result) : base(result)
    {
        AutoSizeAxes = Axes.Both;
    }

    private Bindable<bool> timingIndicatorEnabled = new Bindable<bool>();

    [BackgroundDependencyLoader]
    private void load(SentakkiRulesetConfigManager configManager)
    {
        JudgementText.Text = SentakkiExtensions.GetDisplayNameForSentakkiResult(Result).ToUpperInvariant();
        JudgementText.Colour = colours.ForSentakkiResult(Result);

        if (Result is not HitResult.Perfect && Result.IsHit())
        {
            AddInternal(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = timingIndicatorText = new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.BottomCentre,
                    Blending = BlendingParameters.Additive,
                    Font = OsuFont.Numeric.With(size: 13),
                }
            });

            configManager.BindWith(SentakkiRulesetSettings.DetailedJudgements, timingIndicatorEnabled);
            timingIndicatorEnabled.BindValueChanged(v => timingIndicatorText.Alpha = v.NewValue ? 1 : 0, true);
        }
    }

    public void ApplyTimingIndicatorFor(JudgementResult result)
    {
        if (timingIndicatorText is null)
            return;

        // We don't want to show this to hitobjects that don't have the concept of timing
        timingIndicatorText.Scale = result.HitObject.HitWindows is SentakkiEmptyHitWindows ? Vector2.Zero : Vector2.One;

        bool isEarly = result.TimeOffset < 0;

        timingIndicatorText.Text = isEarly ? "EARLY" : "LATE";
        timingIndicatorText.Colour = isEarly ? Color4.GreenYellow : Color4.OrangeRed;
    }

    public override void PlayAnimation()
    {
        switch (Result)
        {
            case HitResult.None:
                this.FadeOutFromOne(800);
                break;

            case HitResult.Miss:
                this.ScaleTo(1.6f);
                this.ScaleTo(1, 100, Easing.In);

                this.MoveTo(Vector2.Zero);
                this.MoveToOffset(new Vector2(0, 50), 800, Easing.InQuint);

                this.RotateTo(0);
                this.RotateTo(40, 800, Easing.InQuint);

                this.FadeOutFromOne(800);
                break;

            default:
                this.ScaleTo(0.8f);
                this.ScaleTo(1, 50, Easing.OutElastic);

                this.Delay(50)
                    .ScaleTo(0.75f, 300)
                    .FadeOut(300);
                break;
        }
    }
}
