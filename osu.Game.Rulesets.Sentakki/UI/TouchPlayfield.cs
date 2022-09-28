using osu.Framework.Allocation;
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
        private SentakkiInputManager sentakkiActionInputManager = null!;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= (SentakkiInputManager)GetContainingInputManager();

        public TouchPlayfield()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
        }

        [Resolved]
        private DrawableSentakkiRuleset drawableSentakkiRuleset { get; set; } = null!;

        [Resolved]
        private SentakkiRulesetConfigManager? sentakkiRulesetConfig { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RegisterPool<Objects.Touch, DrawableTouch>(8);
        }

        protected override HitObjectLifetimeEntry CreateLifetimeEntry(HitObject hitObject) => new SentakkiHitObjectLifetimeEntry(hitObject, sentakkiRulesetConfig, drawableSentakkiRuleset);

        protected override HitObjectContainer CreateHitObjectContainer() => new TouchHitObjectContainer();

        private TouchHitObjectContainer touchHitObjectContainer => (TouchHitObjectContainer)HitObjectContainer;

        private SortedList<Drawable> aliveTouchNotes => touchHitObjectContainer.AliveTouchNotes;

        protected override void Update()
        {
            base.Update();

            if (aliveTouchNotes.Count <= 0) return;

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
                if (hasAction && touch.ReceivePositionalInputAt(pointerPosition!.Value))
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
            // This is exposed to allow TouchPlayfield to
            public SortedList<Drawable> AliveTouchNotes => (SortedList<Drawable>)AliveInternalChildren;

            protected override bool UpdateChildrenLife()
            {
                return base.UpdateChildrenLife();
            }
        }
    }
}
