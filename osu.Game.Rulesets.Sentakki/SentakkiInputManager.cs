using System.Collections.Generic;
using System.ComponentModel;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki
{
    public class SentakkiInputManager : RulesetInputManager<SentakkiAction>
    {
        protected override bool MapMouseToLatestTouch => false;
        public bool AllowUserPresses
        {
            set => ((SentakkiKeyBindingContainer)KeyBindingContainer).AllowUserPresses = value;
        }

        protected override KeyBindingContainer<SentakkiAction> CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
            => new SentakkiKeyBindingContainer(ruleset, variant, unique);

        private class SentakkiKeyBindingContainer : RulesetKeyBindingContainer
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
        }

        public IEnumerable<SentakkiAction> PressedActions => KeyBindingContainer.PressedActions;

        // For makeshift virtual input handling
        public void TriggerPressed(SentakkiAction action) => KeyBindingContainer.TriggerPressed(action);
        public void TriggerReleased(SentakkiAction action) => KeyBindingContainer.TriggerReleased(action);

        public SentakkiInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }
    }

    public enum SentakkiAction
    {
        [Description("Button 1")]
        Button1,

        [Description("Button 2")]
        Button2,

        [Description("Key 1")]
        Key1,

        [Description("Key 2")]
        Key2,

        [Description("Key 3")]
        Key3,

        [Description("Key 4")]
        Key4,

        [Description("Key 5")]
        Key5,

        [Description("Key 6")]
        Key6,

        [Description("Key 7")]
        Key7,

        [Description("Key 8")]
        Key8,
    }
}
