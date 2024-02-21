using System.Collections.Generic;
using osu.Framework.Extensions.ListExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Framework.Lists;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Sentakki.Localisation;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki
{
    public partial class SentakkiInputManager : RulesetInputManager<SentakkiAction>
    {
        protected override bool MapMouseToLatestTouch => false;

        public bool AllowUserPresses
        {
            set => ((SentakkiKeyBindingContainer)KeyBindingContainer).AllowUserPresses = value;
        }

        protected override KeyBindingContainer<SentakkiAction> CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
            => new SentakkiKeyBindingContainer(ruleset, variant, unique);

        private partial class SentakkiKeyBindingContainer : RulesetKeyBindingContainer
        {
            public bool AllowUserPresses = true;

            public SentakkiKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                : base(ruleset, variant, unique)
            {
            }

            protected override bool Handle(UIEvent e)
            {
                if (!AllowUserPresses) return false;

                return base.Handle(e);
            }

            // We want the press behavior of SimultaneousBindingMode.All, but we want the release behavior of SimultaneousBindingMode.Unique
            // As long as there are more than one input source triggering the action, we manually remove the action from the list once, without propogating the release
            // When the final source is released, we let the original handling take over, which would also propogate the release event
            // This is so that multiple sources (virtual input/key) can trigger a press, but not release until the last key is released
            protected override void PropagateReleased(IEnumerable<Drawable> drawables, InputState state, SentakkiAction released)
            {
                int actionCount = 0;
                var pressed = PressedActions;

                for (int i = 0; i < pressed.Count; ++i)
                {
                    if (pressed[i] == released && ++actionCount > 1)
                        break;
                }

                if (actionCount > 1)
                    pressed.Remove(released);
                else
                    base.PropagateReleased(drawables, state, released);
            }
        }

        public SlimReadOnlyListWrapper<SentakkiAction> PressedActions => KeyBindingContainer.PressedActions;

        // For makeshift virtual input handling
        public void TriggerPressed(SentakkiAction action) => KeyBindingContainer.TriggerPressed(action);
        public void TriggerReleased(SentakkiAction action) => KeyBindingContainer.TriggerReleased(action);

        public SentakkiInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.All)
        {
        }
    }

    public enum SentakkiAction
    {
        [LocalisableDescription(typeof(SentakkiActionStrings), nameof(SentakkiActionStrings.Button1))]
        Button1,

        [LocalisableDescription(typeof(SentakkiActionStrings), nameof(SentakkiActionStrings.Button2))]
        Button2,

        [LocalisableDescription(typeof(SentakkiActionStrings), nameof(SentakkiActionStrings.Key1))]
        Key1,

        [LocalisableDescription(typeof(SentakkiActionStrings), nameof(SentakkiActionStrings.Key2))]
        Key2,

        [LocalisableDescription(typeof(SentakkiActionStrings), nameof(SentakkiActionStrings.Key3))]
        Key3,

        [LocalisableDescription(typeof(SentakkiActionStrings), nameof(SentakkiActionStrings.Key4))]
        Key4,

        [LocalisableDescription(typeof(SentakkiActionStrings), nameof(SentakkiActionStrings.Key5))]
        Key5,

        [LocalisableDescription(typeof(SentakkiActionStrings), nameof(SentakkiActionStrings.Key6))]
        Key6,

        [LocalisableDescription(typeof(SentakkiActionStrings), nameof(SentakkiActionStrings.Key7))]
        Key7,

        [LocalisableDescription(typeof(SentakkiActionStrings), nameof(SentakkiActionStrings.Key8))]
        Key8,
    }
}
