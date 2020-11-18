using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input.Bindings;
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
            AddRangeInternal(new Drawable[]{
                HitObjectContainer,
                new LaneReceptor()
            });
        }

        private DrawableSentakkiRuleset drawableSentakkiRuleset;
        private SentakkiRulesetConfigManager sentakkiRulesetConfig;

        [BackgroundDependencyLoader(true)]
        private void load(DrawableSentakkiRuleset drawableRuleset, SentakkiRulesetConfigManager sentakkiRulesetConfigManager)
        {
            drawableSentakkiRuleset = drawableRuleset;
            sentakkiRulesetConfig = sentakkiRulesetConfigManager;

            registerPool<Tap, DrawableTap>(8);
            registerPool<Hold, DrawableHold>(8);
            registerPool<Slide, DrawableSlide>(2);
        }

        private void registerPool<TObject, TDrawable>(int initialSize, int? maximumSize = null)
            where TObject : HitObject
            where TDrawable : DrawableHitObject, new()
            => RegisterPool<TObject, TDrawable>(CreatePool<TDrawable>(initialSize, maximumSize));

        protected virtual DrawablePool<TDrawable> CreatePool<TDrawable>(int initialSize, int? maximumSize = null)
            where TDrawable : DrawableHitObject, new()
            => new DrawableSentakkiPool<TDrawable>(OnLoaded, initialSize, maximumSize);

        protected override HitObjectLifetimeEntry CreateLifetimeEntry(HitObject hitObject) => new SentakkiHitObjectLifetimeEntry(hitObject, sentakkiRulesetConfig, drawableSentakkiRuleset);

        public class LaneReceptor : CompositeDrawable, IKeyBindingHandler<SentakkiAction>
        {
            private SentakkiInputManager sentakkiActionInputManager;
            internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

            public override bool HandlePositionalInput => true;

            private readonly BindableInt currentKeys = new BindableInt(0);

            public LaneReceptor()
            {
                Position = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, 0);
                Size = new Vector2(300);

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                CornerRadius = 150;
                CornerExponent = 2;
                currentKeys.BindValueChanged(handleKeyPress);
            }

            private readonly Bindable<LaneInputMode> laneInputMode = new Bindable<LaneInputMode>();

            [BackgroundDependencyLoader(true)]
            private void load(SentakkiRulesetConfigManager sentakkiConfig)
            {
                sentakkiConfig?.BindWith(SentakkiRulesetSettings.LaneInputMode, laneInputMode);
            }

            protected override void Update()
            {
                base.Update();
                int count = 0;
                var touchInput = SentakkiActionInputManager.CurrentState.Touch;

                if (touchInput.ActiveSources.Any())
                    count = touchInput.ActiveSources.Where(x => ReceivePositionalInputAt(touchInput.GetTouchPosition(x) ?? new Vector2(float.MinValue))).Count();
                else if (IsHovered && laneInputMode.Value == LaneInputMode.Sensor)
                    count = SentakkiActionInputManager.PressedActions.Where(x => x < SentakkiAction.Key1).Count();

                currentKeys.Value = count;
            }

            private void handleKeyPress(ValueChangedEvent<int> keys)
            {
                if (keys.NewValue > keys.OldValue || keys.NewValue == 0)
                    SentakkiActionInputManager.TriggerReleased(SentakkiAction.Key1 + ((Lane)Parent).LaneNumber);

                if (keys.NewValue > keys.OldValue)
                    SentakkiActionInputManager.TriggerPressed(SentakkiAction.Key1 + ((Lane)Parent).LaneNumber);
            }

            public bool OnPressed(SentakkiAction action)
            {
                if (laneInputMode.Value != LaneInputMode.Button) return false;

                if (action >= SentakkiAction.Key1 || !IsHovered) return false;

                SentakkiActionInputManager.TriggerPressed(SentakkiAction.Key1 + ((Lane)Parent).LaneNumber);
                return false;
            }

            public void OnReleased(SentakkiAction action)
            {
                if (laneInputMode.Value != LaneInputMode.Button) return;

                if (action >= SentakkiAction.Key1) return;

                SentakkiActionInputManager.TriggerReleased(SentakkiAction.Key1 + ((Lane)Parent).LaneNumber);
            }
        }
    }
}
