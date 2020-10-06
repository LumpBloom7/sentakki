using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Judgements;
using System;

public class DrawableScorePaddingObject : DrawableHitObject<ScorePaddingObject>
{
    public DrawableScorePaddingObject(ScorePaddingObject hitObject)
        : base(hitObject)
    {
    }

    public new void ApplyResult(Action<JudgementResult> application)
    {
        if (!Result.HasResult)
            base.ApplyResult(application);
    }
}
