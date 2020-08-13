using osu.Framework.Allocation;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Framework.Bindables;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSentakkiHitObject : DrawableHitObject<SentakkiHitObject>
    {
        private readonly Bindable<bool> userPositionalHitSounds = new Bindable<bool>(false);
        private readonly SkinnableSound breakSound;

        [Resolved(canBeNull: true)]
        private GameplayClock gameplayClock { get; set; }

        public bool IsHidden = false;
        public bool IsFadeIn = false;

        public bool Auto = false;

        // Used in the editor
        public bool IsVisible => Time.Current >= HitObject.StartTime - AnimationDuration.Value;

        // Used for the animation update
        protected readonly Bindable<double> AnimationDuration = new Bindable<double>(1000);
        protected readonly Bindable<double> AdjustedAnimationDuration = new Bindable<double>(1000);

        protected override float SamplePlaybackPosition => (SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, HitObject.Lane).X / (SentakkiPlayfield.INTERSECTDISTANCE * 2)) + .5f;
        public SentakkiAction[] HitActions { get; set; } = new[]
        {
            SentakkiAction.Button1,
            SentakkiAction.Button2,
        };

        public DrawableSentakkiHitObject(SentakkiHitObject hitObject)
            : base(hitObject)
        {
            if (hitObject.IsBreak)
                AddRangeInternal(new Drawable[]{
                    breakSound = new SkinnableSound(new SampleInfo("Break"))
                });
            AdjustedAnimationDuration.BindValueChanged(_ => InvalidateTransforms());
        }

        private DrawableSentakkiRuleset drawableSentakkiRuleset;

        public double GameplaySpeed => drawableSentakkiRuleset?.GameplaySpeed ?? 1;

        private readonly Bindable<bool> breakEnabled = new Bindable<bool>(true);

        [BackgroundDependencyLoader(true)]
        private void load(DrawableSentakkiRuleset drawableRuleset, OsuConfigManager osuConfig, SentakkiRulesetConfigManager sentakkiConfig)
        {
            drawableSentakkiRuleset = drawableRuleset;
            osuConfig.BindWith(OsuSetting.PositionalHitSounds, userPositionalHitSounds);
            sentakkiConfig?.BindWith(SentakkiRulesetSettings.BreakSounds, breakEnabled);
        }

        protected override void Update()
        {
            base.Update();
            AdjustedAnimationDuration.Value = AnimationDuration.Value * GameplaySpeed;
        }

        protected virtual void InvalidateTransforms()
        {
            foreach (var transform in Transforms)
            {
                transform.Apply(double.MinValue);
                RemoveTransform(transform);
            }
            foreach (Drawable internalChild in InternalChildren)
            {
                internalChild.ApplyTransformsAt(double.MinValue, true);
                internalChild.ClearTransforms(true);
            }
            using (BeginAbsoluteSequence(HitObject.StartTime - InitialLifetimeOffset))
            {
                UpdateInitialTransforms();
                double offset = Result?.TimeOffset ?? 0;
                using (BeginDelayedSequence(InitialLifetimeOffset + offset))
                    UpdateStateTransforms(State.Value);
            }
        }

        protected virtual bool PlayBreakSample => true;
        public override void PlaySamples()
        {
            base.PlaySamples();
            if (PlayBreakSample && breakSound != null && Result.Type == HitResult.Perfect && breakEnabled.Value && (!gameplayClock?.IsSeeking ?? false))
            {
                const float balance_adjust_amount = 0.4f;
                breakSound.Balance.Value = balance_adjust_amount * (userPositionalHitSounds.Value ? SamplePlaybackPosition - 0.5f : 0);
                breakSound.Play();
            }
        }
    }
}
