using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.UI;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableSentakkiHitObject : DrawableHitObject<SentakkiHitObject>
    {
        protected override double InitialLifetimeOffset => AdjustedAnimationDuration;

        public readonly BindableBool AutoBindable = new BindableBool();

        public bool Auto
        {
            get => AutoBindable.Value;
            set => AutoBindable.Value = value;
        }

        // Used for the animation update
        protected readonly Bindable<double> AnimationDuration = new Bindable<double>(1000);

        protected override float SamplePlaybackPosition => Position.X / (SentakkiPlayfield.INTERSECTDISTANCE * 2);

        public DrawableSentakkiHitObject()
            : this(null)
        {
        }

        public DrawableSentakkiHitObject(SentakkiHitObject? hitObject = null)
            : base(hitObject!)
        {
        }

        [Resolved]
        private DrawableSentakkiRuleset? drawableSentakkiRuleset { get; set; }

        public double GameplaySpeed => drawableSentakkiRuleset?.GameplaySpeed ?? 1;

        protected double AdjustedAnimationDuration => AnimationDuration.Value * GameplaySpeed;

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();
            AnimationDuration.BindValueChanged(_ => queueTransformReset(), true);
        }

        public Bindable<bool> ExModifierBindable = new Bindable<bool>();

        protected override void OnApply()
        {
            base.OnApply();
            AccentColour.BindTo(HitObject.ColourBindable);
            ExModifierBindable.BindTo(HitObject.ExStateBindable);
        }

        protected void ApplyResult(HitResult result)
        {
            void resultApplication(JudgementResult r) => r.Type = result;
            ApplyResult(resultApplication);
        }

        protected override void OnFree()
        {
            base.OnFree();
            AccentColour.UnbindFrom(HitObject.ColourBindable);
            ExModifierBindable.UnbindFrom(HitObject.ExStateBindable);
        }

        protected override void Update()
        {
            base.Update();

            if (transformResetQueued) RefreshStateTransforms();
        }

        // We need to make sure the current transform resets, perhaps due to animation duration being changed
        // We don't want to reset the transform of all DHOs immediately,
        // since repeatedly resetting transforms of non-present DHO is wasteful
        private void queueTransformReset()
        {
            transformResetQueued = true;
            //LifetimeStart = HitObject.StartTime - InitialLifetimeOffset;
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
            if (!Result.HasResult)
                base.ApplyResult(application);
        }
    }
}
