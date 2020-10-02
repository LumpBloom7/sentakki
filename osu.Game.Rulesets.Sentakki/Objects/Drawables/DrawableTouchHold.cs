using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;
using System.Linq;
using System;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableTouchHold : DrawableSentakkiTouchHitObject
    {
        private readonly TouchHoldBody touchHoldBody;

        public override bool HandlePositionalInput => true;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        protected override double InitialLifetimeOffset => 4000;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => touchHoldBody.ReceivePositionalInputAt(screenSpacePos);

        public DrawableTouchHold(TouchHold hitObject)
            : base(hitObject)
        {
            Colour = Color4.SlateGray;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = new Vector2(0f);
            Alpha = 0;
            AlwaysPresent = true;
            AddRangeInternal(new Drawable[] {
                touchHoldBody = new TouchHoldBody(){ Duration = hitObject.Duration },
            });

            isHitting.BindValueChanged(b =>
            {
                if (b.NewValue) beginHold();
                else endHold();
            });
        }

        [Resolved]
        private OsuColour colours { get; set; }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current < HitObject.StartTime) return;

            if (userTriggered || Time.Current < (HitObject as IHasDuration)?.EndTime)
                return;

            double result = totalHoldTime / (HitObject as IHasDuration).Duration;

            HitResult resultType;

            if (result >= .75)
                resultType = HitResult.Great;
            else if (result >= .5)
                resultType = HitResult.Good;
            else if (result >= .25)
                resultType = HitResult.Meh;
            else
                resultType = HitResult.Miss;

            AccentColour.Value = colours.ForHitResult(resultType);

            ApplyResult(r => r.Type = resultType);
        }

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfigs)
        {
            sentakkiConfigs?.BindWith(SentakkiRulesetSettings.TouchAnimationDuration, AnimationDuration);
        }

        protected override void UpdateInitialTransforms()
        {
            double fadeIn = AnimationDuration.Value * GameplaySpeed;
            using (BeginAbsoluteSequence(HitObject.StartTime - fadeIn, true))
            {
                this.FadeInFromZero(fadeIn).ScaleTo(1, fadeIn);
                using (BeginDelayedSequence(fadeIn, true))
                {
                    touchHoldBody.ProgressPiece.TransformBindableTo(touchHoldBody.ProgressPiece.ProgressBindable, 1, ((IHasDuration)HitObject).Duration);
                }
            }
        }

        private readonly Bindable<bool> isHitting = new Bindable<bool>();

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        private double? holdStartTime;
        private double totalHoldTime;

        private void beginHold()
        {
            holdStartTime = Math.Max(Time.Current, HitObject.StartTime);
            Colour = Color4.White;
        }

        private void endHold()
        {
            if (holdStartTime.HasValue)
                totalHoldTime += Math.Max(Time.Current - holdStartTime.Value, 0);

            holdStartTime = null;
            Colour = Color4.SlateGray;
        }


        protected override void Update()
        {
            base.Update();

            var touchInput = SentakkiActionInputManager.CurrentState.Touch;
            bool isTouched = touchInput.ActiveSources.Any(s => ReceivePositionalInputAt(touchInput.GetTouchPosition(s) ?? new Vector2(float.MinValue)));
            isHitting.Value = Time.Current >= HitObject.StartTime
                            && Time.Current <= (HitObject as IHasDuration)?.EndTime
                            && (Auto || AutoTouch || isTouched || ((SentakkiActionInputManager?.PressedActions.Any() ?? false) && IsHovered));
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);
            const double time_fade_hit = 400, time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    this.Delay((HitObject as IHasDuration).Duration + time_fade_hit).Expire();
                    break;

                case ArmedState.Miss:
                    this.Delay((HitObject as IHasDuration).Duration).ScaleTo(.0f, time_fade_miss).FadeOut(time_fade_miss);
                    break;
            }
        }
    }
}
