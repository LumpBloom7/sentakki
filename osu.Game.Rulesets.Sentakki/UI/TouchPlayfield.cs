using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ListExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Lists;
using osu.Game.Rulesets.Objects;
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

        protected override HitObjectContainer CreateHitObjectContainer() => new TouchHitObjectContainer();

        private TouchHitObjectContainer touchHitObjectContainer => (TouchHitObjectContainer)HitObjectContainer;
        private IEnumerable<DrawableTouch> aliveTouchNotes => touchHitObjectContainer.AliveTouchNotes;

        protected override void Update()
        {
            base.Update();

            if (!aliveTouchNotes.Any()) return;

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

            handlePointInput(0, actionPressed, mousePosition);

            // Handle touch
            for (int i = 0; i < 10; ++i)
            {
                var currentTouchPosition = SentakkiActionInputManager.CurrentState.Touch.GetTouchPosition((TouchSource)i);
                handlePointInput(i + 1, currentTouchPosition.HasValue, currentTouchPosition);
            }
        }

        private void handlePointInput(int pointID, bool hasAction, Vector2? pointerPosition)
        {
            bool continueEventPropogation = true;
            foreach (DrawableTouch touch in aliveTouchNotes)
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

        // This HOC is specially built accommodate the custom input required to handle touch (even though I think the beatmap conversion is at fault)
        // This HOC provides a completely tangible list of objects updated every time hitobjects life changes, rather than a query to fetch all objects
        private class TouchHitObjectContainer : HitObjectContainer
        {
            // This list is exposed to the playfield, so that it can get a list of all objects
            // To prevent this query from being executed 11 times in a single input handling cycle
            // This updates when notes become alive/dead, instead of letting the playfield touch handler from polling every frame
            public SlimReadOnlyListWrapper<DrawableTouch> AliveTouchNotes { get; private set; } = new List<DrawableTouch>().AsSlimReadOnly();

            protected override bool UpdateChildrenLife()
            {
                if (base.UpdateChildrenLife())
                {
                    AliveTouchNotes = AliveObjects.OfType<DrawableTouch>().ToList().AsSlimReadOnly();
                    return true;
                }
                return false;
            }
        }
    }
}
