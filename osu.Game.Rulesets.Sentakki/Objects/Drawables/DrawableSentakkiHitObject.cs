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
        private readonly PausableSkinnableSound breakSound;

        public bool IsHidden = false;
        public bool IsFadeIn = false;

        // Used in the editor
        public bool IsVisible => Time.Current >= HitObject.StartTime - AnimationDuration.Value;

        // Used for the animation update
        protected readonly Bindable<double> AnimationDuration = new Bindable<double>(1000);
        protected readonly Bindable<double> AdjustedAnimationDuration = new Bindable<double>(1000);

        protected override float SamplePlaybackPosition
        {
            get
            {
                if (HitObject is SentakkiLanedHitObject x)
                    return (SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, x.Lane).X / (SentakkiPlayfield.INTERSECTDISTANCE * 2)) + .5f;
                else
                    return Position.X / (SentakkiPlayfield.INTERSECTDISTANCE * 2);
            }
        }

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
                    breakSound = new PausableSkinnableSound(new SampleInfo("Break")),
                });
            AddInternal(scorePaddingObjects = new Container<DrawableScorePaddingObject>());
            AdjustedAnimationDuration.BindValueChanged(_ => InvalidateTransforms());
        }

        private DrawableSentakkiRuleset drawableSentakkiRuleset;

        public double GameplaySpeed => drawableSentakkiRuleset?.GameplaySpeed ?? 1;

        private readonly Bindable<bool> breakEnabled = new Bindable<bool>(true);

        [BackgroundDependencyLoader(true)]
        private void load(DrawableSentakkiRuleset drawableRuleset, SentakkiRulesetConfigManager sentakkiConfig)
        {
            drawableSentakkiRuleset = drawableRuleset;
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
            if (PlayBreakSample && breakSound != null && Result.Type == Result.Judgement.MaxResult && breakEnabled.Value)
            {
                breakSound.Balance.Value = CalculateSamplePlaybackBalance(SamplePlaybackPosition);
                breakSound.Play();
            }
        }

        private Container<DrawableScorePaddingObject> scorePaddingObjects;

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            scorePaddingObjects.Clear();
        }
        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);
            if (hitObject is DrawableScorePaddingObject x)
                scorePaddingObjects.Add(x);
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            if (hitObject is ScorePaddingObject x)
                return new DrawableScorePaddingObject(x);

            return base.CreateNestedHitObject(hitObject);
        }

        protected new void ApplyResult(Action<JudgementResult> application)
        {
            // Apply judgement to this object
            base.ApplyResult(application);

            // Also give Break note score padding a judgement
            foreach (var breakObj in scorePaddingObjects)
                breakObj.ApplyResult(application);
        }
    }
}
