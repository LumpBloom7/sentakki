using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Skinning;
using osuTK;
using SharpGen.Runtime;
using static osu.Game.Rulesets.Sentakki.Extensions.SentakkiExtensions;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public partial class LegacySentakkiJudgementPiece : CompositeDrawable, IAnimatableJudgement, IHasTimingIndicator
{
    private HitResult hitResult;

    private Drawable? judgementDrawable;
    private Drawable? timingIndicatorDrawableEarly;
    private Drawable? timingIndicatorDrawableLate;

    public LegacySentakkiJudgementPiece(HitResult hitResult)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        AutoSizeAxes = Axes.Both;

        this.hitResult = hitResult;
    }

    private Bindable<bool> timingIndicatorEnabled = new Bindable<bool>();

    [BackgroundDependencyLoader]
    private void load(ISkinSource skin, SentakkiRulesetConfigManager configManager)
    {
        judgementDrawable = skin.GetAnimation($"sentakki/judgement-{hitResult.GetDisplayNameForSentakkiResult()}", true, false);

        if (judgementDrawable is not null)
            AddInternal(judgementDrawable);

        if (hitResult is HitResult.Perfect || hitResult.IsMiss())
            return;

        Container timingIndicatorContainer;

        AddInternal(timingIndicatorContainer = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
        });

        timingIndicatorDrawableEarly = skin.GetAnimation($"sentakki/judgement-early", true, false);

        if (timingIndicatorDrawableEarly is not null)
            timingIndicatorContainer.Add(timingIndicatorDrawableEarly);

        timingIndicatorDrawableLate = skin.GetAnimation($"sentakki/judgement-late", true, false);

        if (timingIndicatorDrawableLate is not null)
            timingIndicatorContainer.Add(timingIndicatorDrawableLate);

        configManager.BindWith(SentakkiRulesetSettings.DetailedJudgements, timingIndicatorEnabled);
        timingIndicatorEnabled.BindValueChanged(v => timingIndicatorContainer.Alpha = v.NewValue ? 1 : 0, true);
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

    public void ApplyTimingIndicatorFor(JudgementResult judgementResult)
    {
        if (timingIndicatorDrawableEarly is null)
            return;

        if (timingIndicatorDrawableEarly is not null)
            timingIndicatorDrawableEarly.Scale = judgementResult.TimeOffset < 0 ? Vector2.One : Vector2.Zero;

        if (timingIndicatorDrawableLate is not null)
            timingIndicatorDrawableLate.Scale = judgementResult.TimeOffset < 0 ? Vector2.Zero : Vector2.One;
    }
}
