using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI
{
    // A special playfield specifically made for TouchNotes
    // Contains extra functionality to better propogate touch input to Touch notes, and avoids some double hit weirdness
    public class TouchPlayfield : Playfield
    {
        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

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

            var aliveObjects = HitObjectContainer.AliveObjects.ToList();

            // Handle mouse input
            var mousePosition = SentakkiActionInputManager.CurrentState.Mouse.Position;
            bool actionPressed = false;
            foreach (var action in SentakkiActionInputManager.PressedActions)
            {
                if (action < SentakkiAction.Key1)
                {
                    actionPressed = true;
                    break;
                }
            }

            handlePointInput(0, actionPressed, mousePosition, aliveObjects);

            // Handle touch
            for (int i = 0; i < 10; ++i)
            {
                var currentTouchPosition = SentakkiActionInputManager.CurrentState.Touch.GetTouchPosition((TouchSource)i);
                handlePointInput(i + 1, currentTouchPosition.HasValue, currentTouchPosition, aliveObjects);
            }
        }

        private void handlePointInput(int pointID, bool hasAction, Vector2? pointerPosition, IEnumerable<DrawableHitObject> aliveObjects)
        {
            bool continueEventPropogation = true;
            foreach (DrawableTouch touch in aliveObjects)
            {
                if (hasAction && touch.ReceivePositionalInputAt(pointerPosition.Value))
                {
                    if (!touch.PointInteractionState[pointID])
                    {
                        touch.PointInteractionState[pointID] = true;
                        if (continueEventPropogation)
                            continueEventPropogation = !touch.OnNewPointInteraction();
                    }
                }
                else
                {
                    touch.PointInteractionState[pointID] = false;
                }
            }
        }
    }
}
