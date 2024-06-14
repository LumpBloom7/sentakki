using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.UI;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableSentakkiLanedHitObject : DrawableSentakkiHitObject
    {
        public new SentakkiLanedHitObject HitObject => (SentakkiLanedHitObject)base.HitObject;

        protected override float SamplePlaybackPosition =>
            (SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, HitObject.Lane).X / (SentakkiPlayfield.INTERSECTDISTANCE * 2)) + .5f;

        public DrawableSentakkiLanedHitObject(SentakkiLanedHitObject? hitObject)
            : base(hitObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (DrawableSentakkiRuleset is not null)
                AnimationDuration.BindTo(DrawableSentakkiRuleset?.AdjustedAnimDuration);
        }
    }
}
