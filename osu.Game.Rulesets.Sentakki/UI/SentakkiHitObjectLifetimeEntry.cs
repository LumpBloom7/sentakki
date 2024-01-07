using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class SentakkiHitObjectLifetimeEntry : HitObjectLifetimeEntry
    {
        protected override double InitialLifetimeOffset => AnimationDurationBindable.Value;
        private readonly DrawableSentakkiRuleset drawableRuleset;

        public BindableDouble AnimationDurationBindable = new BindableDouble(1000);

        public SentakkiHitObjectLifetimeEntry(HitObject hitObject, DrawableSentakkiRuleset senRuleset)
            : base(hitObject)
        {
            drawableRuleset = senRuleset;
            bindAnimationDuration();
            AnimationDurationBindable.BindValueChanged(x => LifetimeStart = HitObject.StartTime - InitialLifetimeOffset, true);
        }

        private void bindAnimationDuration()
        {
            switch (HitObject)
            {
                case SentakkiLanedHitObject:
                    AnimationDurationBindable.BindTo(drawableRuleset.AdjustedAnimDuration);
                    break;

                case Touch:
                case TouchHold:
                    AnimationDurationBindable.BindTo(drawableRuleset.AdjustedTouchAnimDuration);
                    break;
            }
        }
    }
}
