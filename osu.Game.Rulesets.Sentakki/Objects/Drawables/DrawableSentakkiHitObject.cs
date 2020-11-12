using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSentakkiHitObject : DrawableHitObject<SentakkiHitObject>
    {
        protected override double InitialLifetimeOffset => AdjustedAnimationDuration;

        public readonly BindableBool AutoBindable = new BindableBool(false);
        public bool Auto
        {
            get => AutoBindable.Value;
            set => AutoBindable.Value = value;
        }

        // Used for the animation update
        protected readonly Bindable<double> AnimationDuration = new Bindable<double>(1000);

        protected override float SamplePlaybackPosition => Position.X / (SentakkiPlayfield.INTERSECTDISTANCE * 2);

        public DrawableSentakkiHitObject(SentakkiHitObject hitObject)
            : base(hitObject)
        {
        }

        private DrawableSentakkiRuleset drawableSentakkiRuleset;

        public double GameplaySpeed => drawableSentakkiRuleset?.GameplaySpeed ?? 1;

        protected double AdjustedAnimationDuration => AnimationDuration.Value * GameplaySpeed;

        [BackgroundDependencyLoader(true)]
        private void load(DrawableSentakkiRuleset drawableRuleset)
        {
            drawableSentakkiRuleset = drawableRuleset;
        }

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();
            AnimationDuration.BindValueChanged(_ => queueTransformReset(), true);
        }

        protected override void Update()
        {
            base.Update();

            // Using SkinChanged to achieve our goals of resetting the transforms,
            // since that is the only publicly accessible function that calls the base Clear/Apply Transforms
            // We also avoid doing this if the state is not idle, since there is little benefit in doing so
            // And it avoids drawable pieces not having the correct state
            // And it avoids samples repeatedly playing again and again, while sliding the slider
            if (transformResetQueued && State.Value == ArmedState.Idle) SkinChanged(CurrentSkin, true);
        }

        // We need to make sure the current transform resets, perhaps due to animation duration being changed
        // We don't want to reset the transform of all DHOs immediately,
        // since repeatedly resetting transforms of non-present DHO is wasteful
        private void queueTransformReset()
        {
            transformResetQueued = true;
            LifetimeStart = HitObject.StartTime - InitialLifetimeOffset;
        }

        protected override void UpdateInitialTransforms()
        {
            // The transform is reset as soon as this function begins
            // This includes the usual LoadComplete() call, or rewind resets
            transformResetQueued = false;
        }

        private bool transformResetQueued;

        protected new virtual void ApplyResult(Action<JudgementResult> application)
        {
            // Apply judgement to this object
            if (Result.HasResult) return;
            base.ApplyResult(application);
        }
    }
}
