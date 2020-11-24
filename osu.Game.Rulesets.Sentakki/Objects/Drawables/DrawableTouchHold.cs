using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableTouchHold : DrawableSentakkiHitObject
    {
        public override bool HandlePositionalInput => true;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => touchHoldBody.ReceivePositionalInputAt(screenSpacePos);

        private TouchHoldBody touchHoldBody;

        public DrawableTouchHold() : this(null) { }

        public DrawableTouchHold(TouchHold hitObject)
            : base(hitObject) { }

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfigs)
        {
            sentakkiConfigs?.BindWith(SentakkiRulesetSettings.TouchAnimationDuration, AnimationDuration);
            Colour = Color4.SlateGray;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = new Vector2(0f);
            Alpha = 0;
            AddRangeInternal(new Drawable[] {
                touchHoldBody = new TouchHoldBody(),
            });

            isHitting.BindValueChanged(b =>
            {
                if (b.NewValue) beginHold();
                else endHold();
            });
        }

        [Resolved]
        private OsuColour colours { get; set; }

        protected override void OnFree(HitObject hitObject)
        {
            base.OnFree(hitObject);

            holdStartTime = null;
            totalHoldTime = 0;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            double fadeIn = AdjustedAnimationDuration;
            this.FadeInFromZero(fadeIn).ScaleTo(1, fadeIn);
            using (BeginDelayedSequence(fadeIn, true))
            {
                touchHoldBody.ProgressPiece.TransformBindableTo(touchHoldBody.ProgressPiece.ProgressBindable, 1, ((IHasDuration)HitObject).Duration);
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
                            && (Auto || isTouched || ((SentakkiActionInputManager?.PressedActions.Any() ?? false) && IsHovered));
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current < (HitObject as IHasDuration)?.EndTime) return;

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

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);
            const double time_fade_hit = 400, time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    this.Delay(time_fade_hit).Expire();
                    break;

                case ArmedState.Miss:
                    this.ScaleTo(.0f, time_fade_miss).FadeOut(time_fade_miss).Expire();
                    break;
            }
        }
    }
}
