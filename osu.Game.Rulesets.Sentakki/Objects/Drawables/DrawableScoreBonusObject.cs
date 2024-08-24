using System;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableScoreBonusObject : DrawableHitObject<ScoreBonusObject>
    {
        public DrawableScoreBonusObject()
            : this(null)
        {
        }

        public DrawableScoreBonusObject(ScoreBonusObject? hitObject)
            : base(hitObject!)
        {
        }

        public void TriggerResult(bool isCritical)
        {
            ApplyResult(isCritical ? HitResult.LargeBonus : HitResult.IgnoreMiss);
        }
    }
}
