using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Lists;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Sentakki.Localisation;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki;

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
