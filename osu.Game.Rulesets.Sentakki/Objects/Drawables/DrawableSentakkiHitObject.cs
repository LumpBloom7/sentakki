using osu.Framework.Allocation;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Framework.Bindables;
using osu.Game.Screens.Play;
using System.Collections.Generic;
using osu.Game.Rulesets.Judgements;
using System.Linq;
using osu.Framework.Graphics.Containers;
using System;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSentakkiHitObject : DrawableHitObject<SentakkiHitObject>
    {

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
        protected readonly Bindable<double> AdjustedAnimationDuration = new Bindable<double>(1000);

        protected override float SamplePlaybackPosition => Position.X / (SentakkiPlayfield.INTERSECTDISTANCE * 2);

        public DrawableSentakkiHitObject(SentakkiHitObject hitObject)
            : base(hitObject)
        {
            AdjustedAnimationDuration.BindValueChanged(_ => InvalidateTransforms());
        }

        private DrawableSentakkiRuleset drawableSentakkiRuleset;

        public double GameplaySpeed => drawableSentakkiRuleset?.GameplaySpeed ?? 1;


        [BackgroundDependencyLoader(true)]
        private void load(DrawableSentakkiRuleset drawableRuleset)
        {
            drawableSentakkiRuleset = drawableRuleset;
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

        protected new virtual void ApplyResult(Action<JudgementResult> application)
        {
            // Apply judgement to this object
            if (Result.HasResult) return;
            base.ApplyResult(application);
        }
    }
}
