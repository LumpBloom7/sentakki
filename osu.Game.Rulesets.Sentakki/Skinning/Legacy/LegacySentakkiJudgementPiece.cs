using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using static osu.Game.Rulesets.Sentakki.Extensions.SentakkiExtensions;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public partial class LegacySentakkiJudgementPiece : CompositeDrawable, IAnimatableJudgement
{
    private HitResult hitResult;

    private Drawable? judgementDrawable;

    public LegacySentakkiJudgementPiece(HitResult hitResult)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        AutoSizeAxes = Axes.Both;

        this.hitResult = hitResult;
    }

    [BackgroundDependencyLoader]
    private void load(ISkinSource skin)
    {
        judgementDrawable = skin.GetAnimation($"sentakki/judgement-{hitResult.GetDisplayNameForSentakkiResult()}", true, false);

        if (judgementDrawable is null)
            return;

        AddInternal(judgementDrawable);
    }

    public Drawable? GetAboveHitObjectsProxiedContent() => CreateProxy();

    public void PlayAnimation()
    {
        (judgementDrawable as IFramedAnimation)?.GotoFrame(0);

        switch (hitResult)
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
