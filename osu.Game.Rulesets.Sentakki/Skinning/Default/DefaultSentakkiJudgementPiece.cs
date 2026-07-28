using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Extensions;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default;

public partial class DefaultSentakkiJudgementPiece(HitResult hitResult) : DefaultJudgementPiece(hitResult)
{
    [Resolved]
    private OsuColour colours { get; set; } = null!;

    protected override void LoadComplete()
    {
        base.LoadComplete();

        JudgementText.Text = SentakkiExtensions.GetDisplayNameForSentakkiResult(Result).ToUpperInvariant();
        JudgementText.Colour = colours.ForSentakkiResult(Result);
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
