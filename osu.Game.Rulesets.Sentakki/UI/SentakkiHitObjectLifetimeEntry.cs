using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Objects;
using osu.Framework.Bindables;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Configuration;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class SentakkiHitObjectLifetimeEntry : HitObjectLifetimeEntry
    {
        protected override double InitialLifetimeOffset => initialLifetimeOffsetFor(HitObject);

        private DrawableSentakkiRuleset drawableRuleset;

        public double GameplaySpeed => drawableRuleset?.GameplaySpeed ?? 1;

        public BindableDouble AnimationDurationBindable = new BindableDouble(1000);

        protected double AdjustedAnimationDuration => AnimationDurationBindable.Value * GameplaySpeed;

        private SentakkiRulesetConfigManager sentakkiConfigs;

        public SentakkiHitObjectLifetimeEntry(HitObject hitObject, SentakkiRulesetConfigManager configManager, DrawableSentakkiRuleset senRuleset) : base(hitObject)
        {
            sentakkiConfigs = configManager;
            drawableRuleset = senRuleset;
            bindAnimationDuration();
            AnimationDurationBindable.BindValueChanged(x =>
            {
                LifetimeStart = HitObject.StartTime - InitialLifetimeOffset;
            });
        }

        private void bindAnimationDuration()
        {
            switch (HitObject)
            {
                case SentakkiLanedHitObject _:
                    sentakkiConfigs?.BindWith(SentakkiRulesetSettings.AnimationDuration, AnimationDurationBindable);
                    break;
                case Touch _:
                case TouchHold _:
                    sentakkiConfigs?.BindWith(SentakkiRulesetSettings.TouchAnimationDuration, AnimationDurationBindable);
                    break;
            }
        }

        private double initialLifetimeOffsetFor(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Touch _:
                    return AdjustedAnimationDuration + HitObject.HitWindows.WindowFor(HitResult.Meh);
                default:
                    return AdjustedAnimationDuration;
            }
        }
    }
}
