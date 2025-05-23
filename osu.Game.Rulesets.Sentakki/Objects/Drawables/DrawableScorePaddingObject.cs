using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableScorePaddingObject : DrawableHitObject<ScorePaddingObject>
    {
        public DrawableScorePaddingObject()
            : this(null)
        {
        }

        public DrawableScorePaddingObject(ScorePaddingObject? hitObject)
            : base(hitObject!)
        {
        }

        public new void ApplyResult(HitResult result) => base.ApplyResult(result);
    }
}
