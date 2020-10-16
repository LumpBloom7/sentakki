using osu.Framework.Allocation;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using System;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSentakkiHitObject : DrawableHitObject<SentakkiHitObject>
    {
        protected override double InitialLifetimeOffset => AdjustedAnimationDuration;

        public bool IsHidden = false;
        public bool IsFadeIn = false;

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
            AnimationDuration.BindValueChanged(_ => queueTransformReset());
        }

        private DrawableSentakkiRuleset drawableSentakkiRuleset;

        public double GameplaySpeed => drawableSentakkiRuleset?.GameplaySpeed ?? 1;

        protected double AdjustedAnimationDuration => AnimationDuration.Value * GameplaySpeed;


        [BackgroundDependencyLoader(true)]
        private void load(DrawableSentakkiRuleset drawableRuleset)
        {
            drawableSentakkiRuleset = drawableRuleset;
        }

        protected override void Update()
        {
            base.Update();
            if (transformResetQueued) ResetTransforms();
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

        protected virtual void ResetTransforms()
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
            using (BeginAbsoluteSequence(HitObject.StartTime - InitialLifetimeOffset, true))
            {
                UpdateInitialTransforms();
                double offset = Result?.TimeOffset ?? 0;
                using (BeginDelayedSequence(InitialLifetimeOffset + offset, true))
                    UpdateStateTransforms(State.Value);
            }
        }

        protected new virtual void ApplyResult(Action<JudgementResult> application)
        {
            // Apply judgement to this object
            if (Result.HasResult) return;
            base.ApplyResult(application);
        }
    }
}
