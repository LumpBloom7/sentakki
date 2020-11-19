using System;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableScorePaddingObject : DrawableHitObject<ScorePaddingObject>
    {
        public DrawableScorePaddingObject() : this(null) { }

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
}
