using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class Lane : Playfield
    {
        public int LaneNumber { get; set; }

        public Action<Drawable> OnLoaded;

        public Lane()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.None;
            AddInternal(HitObjectContainer);

            currentKeys.ValueChanged += handleKeyPress;
        }

        protected override void Update()
        {
            base.Update();
            updateInputState();
        }

        private DrawableSentakkiRuleset drawableSentakkiRuleset;
        private SentakkiRulesetConfigManager sentakkiRulesetConfig;

        [BackgroundDependencyLoader(true)]
        private void load(DrawableSentakkiRuleset drawableRuleset, SentakkiRulesetConfigManager sentakkiRulesetConfigManager)
        {
            drawableSentakkiRuleset = drawableRuleset;
            sentakkiRulesetConfig = sentakkiRulesetConfigManager;

            RegisterPool<Tap, DrawableTap>(8);

            RegisterPool<Hold, DrawableHold>(8);
            RegisterPool<Hold.HoldHead, DrawableHoldHead>(8);

            RegisterPool<Slide, DrawableSlide>(2);
            RegisterPool<SlideTap, DrawableSlideTap>(2);
            RegisterPool<SlideBody, DrawableSlideBody>(2);
            RegisterPool<SlideBody.SlideNode, DrawableSlideNode>(18);

            RegisterPool<ScorePaddingObject, DrawableScorePaddingObject>(20);
        }

        protected override void OnNewDrawableHitObject(DrawableHitObject d) => OnLoaded?.Invoke(d);

        protected override HitObjectLifetimeEntry CreateLifetimeEntry(HitObject hitObject) => new SentakkiHitObjectLifetimeEntry(hitObject, sentakkiRulesetConfig, drawableSentakkiRuleset);

        #region Input Handling
        private const float receptor_angle_range = 45 * 1.4f;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        public override bool HandlePositionalInput => true;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            var localPos = ToLocalSpace(screenSpacePos);

            var angleDelta = SentakkiExtensions.GetDeltaAngle(0, Vector2.Zero.GetDegreesFromPosition(localPos));
            if (Math.Abs(angleDelta) > receptor_angle_range / 2) return false;

            var distance = Vector2.DistanceSquared(Vector2.Zero, localPos);
            if (distance < 200 * 200 || distance > 400 * 400) return false;

            return true;
        }

        private readonly BindableInt currentKeys = new BindableInt(0);

        private bool usingSensor => drawableSentakkiRuleset.UseSensorMode;

        private bool isBeingClicked;

        private void updateInputState()
        {
            int count = isBeingClicked ? 1 : 0;
            var touchInput = SentakkiActionInputManager.CurrentState.Touch;

            if (touchInput.ActiveSources.Any())
            {
                foreach (var t in touchInput.ActiveSources)
                    if (ReceivePositionalInputAt(touchInput.GetTouchPosition(t).Value)) ++count;
            }
            else if (IsHovered && usingSensor)
            {
                foreach (var a in SentakkiActionInputManager.PressedActions)
                    if (a < SentakkiAction.Key1) ++count;
            }

            currentKeys.Value = count;
        }

        private void handleKeyPress(ValueChangedEvent<int> keys)
        {
            if (keys.NewValue > keys.OldValue || keys.NewValue == 0)
                SentakkiActionInputManager.TriggerReleased(SentakkiAction.Key1 + LaneNumber);

            if (keys.NewValue > keys.OldValue)
                SentakkiActionInputManager.TriggerPressed(SentakkiAction.Key1 + LaneNumber);
        }

        public bool OnPressed(SentakkiAction action)
        {
            if (usingSensor) return false;

            if (action >= SentakkiAction.Key1 || !IsHovered) return false;

            isBeingClicked = true;
            return false;
        }

        public void OnReleased(SentakkiAction action)
        {
            if (usingSensor) return;

            if (action >= SentakkiAction.Key1) return;

            isBeingClicked = false;
        }
        #endregion
    }
}
