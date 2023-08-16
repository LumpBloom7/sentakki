using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class SentakkiHitObjectLifetimeEntry : HitObjectLifetimeEntry
    {
        protected override double InitialLifetimeOffset
            => HitObject switch
            {
                Touch => AdjustedAnimationDuration + HitObject.HitWindows.WindowFor(HitResult.Great),
                _ => AdjustedAnimationDuration
            };

        private readonly DrawableSentakkiRuleset drawableRuleset;

        public double GameplaySpeed => drawableRuleset?.GameplaySpeed ?? 1;

        public BindableDouble AnimationDurationBindable = new BindableDouble(1000);

        protected double AdjustedAnimationDuration => AnimationDurationBindable.Value * GameplaySpeed;

        private readonly SentakkiRulesetConfigManager? sentakkiConfigs;

        public SentakkiHitObjectLifetimeEntry(HitObject hitObject, SentakkiRulesetConfigManager? configManager, DrawableSentakkiRuleset senRuleset)
            : base(hitObject)
        {
            sentakkiConfigs = configManager;
            drawableRuleset = senRuleset;
            bindAnimationDuration();
            AnimationDurationBindable.BindValueChanged(x => LifetimeStart = HitObject.StartTime - InitialLifetimeOffset, true);
        }

        private void bindAnimationDuration()
        {
            switch (HitObject)
            {
                case SentakkiLanedHitObject:
                    sentakkiConfigs?.BindWith(SentakkiRulesetSettings.AnimationDuration, AnimationDurationBindable);
                    break;

                case Touch:
                case TouchHold:
                    sentakkiConfigs?.BindWith(SentakkiRulesetSettings.TouchAnimationDuration, AnimationDurationBindable);
                    break;
            }
        }
    }
}
