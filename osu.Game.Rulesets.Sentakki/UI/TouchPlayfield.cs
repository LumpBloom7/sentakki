using System.Buffers;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class TouchPlayfield : Playfield
    {
        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        public override bool HandlePositionalInput => true;

        public TouchPlayfield()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
        }

        private DrawableSentakkiRuleset drawableSentakkiRuleset;
        private SentakkiRulesetConfigManager sentakkiRulesetConfig;

        [BackgroundDependencyLoader(true)]
        private void load(DrawableSentakkiRuleset drawableRuleset, SentakkiRulesetConfigManager sentakkiRulesetConfigManager)
        {
            drawableSentakkiRuleset = drawableRuleset;
            sentakkiRulesetConfig = sentakkiRulesetConfigManager;

            RegisterPool<Objects.Touch, DrawableTouch>(8);
        }

        protected override HitObjectLifetimeEntry CreateLifetimeEntry(HitObject hitObject) => new SentakkiHitObjectLifetimeEntry(hitObject, sentakkiRulesetConfig, drawableSentakkiRuleset);

        protected override void Update()
        {
            base.Update();

            // Handle mouse input
            bool continueMouseEventPropogation = true;
            var mousePosition = SentakkiActionInputManager.CurrentState.Mouse.Position;
            var actionPressed = false;
            foreach (var action in SentakkiActionInputManager.PressedActions)
            {
                if (action < SentakkiAction.Key1)
                {
                    actionPressed = true;
                    break;
                }
            }

            foreach (DrawableTouch touch in HitObjectContainer.AliveObjects)
            {
                if (actionPressed && touch.ReceivePositionalInputAt(mousePosition))
                {
                    if (!touch.IsPointOver[0])
                    {
                        touch.IsPointOver[0] = true;
                        if (continueMouseEventPropogation)
                            continueMouseEventPropogation = !touch.OnNewHeldPointDetected();
                    }
                }
                else
                {
                    touch.IsPointOver[0] = false;
                }
            }

            // Handle touch
            for (int i = 0; i < 10; ++i)
            {
                bool continueTouchEventPropogation = true;
                Vector2? currentTouchPosition = SentakkiActionInputManager.CurrentState.Touch.GetTouchPosition((TouchSource)i);

                foreach (DrawableTouch touch in HitObjectContainer.AliveObjects)
                {
                    if (currentTouchPosition.HasValue && touch.ReceivePositionalInputAt(currentTouchPosition.Value))
                    {
                        if (!touch.IsPointOver[i + 1])
                        {
                            touch.IsPointOver[i + 1] = true;
                            if (continueTouchEventPropogation)
                                continueTouchEventPropogation = !touch.OnNewHeldPointDetected();
                        }
                    }
                    else
                    {
                        touch.IsPointOver[i + 1] = false;
                    }
                }
            }
        }
    }
}
