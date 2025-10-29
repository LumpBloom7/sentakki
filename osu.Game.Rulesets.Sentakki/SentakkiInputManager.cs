using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Lists;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Sentakki.Localisation;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Sentakki;

[Cached]
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
        : base(ruleset, 0, SimultaneousBindingMode.Unique)
    {
    }
}

public enum SentakkiAction
{
    [LocalisableDescription(typeof(SentakkiActionStrings), nameof(SentakkiActionStrings.Button1))]
    Button1,

    [LocalisableDescription(typeof(SentakkiActionStrings), nameof(SentakkiActionStrings.Button2))]
    Button2,

    // These represent potential ring buttons
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

    // These are meant for touch screen usage
    SensorLane1,
    SensorLane2,
    SensorLane3,
    SensorLane4,
    SensorLane5,
    SensorLane6,
    SensorLane7,
    SensorLane8,

    SensorLane1Alt,
    SensorLane2Alt,
    SensorLane3Alt,
    SensorLane4Alt,
    SensorLane5Alt,
    SensorLane6Alt,
    SensorLane7Alt,
    SensorLane8Alt,

    // These are auxiliary actions for mouse + Keyboard users
    B1Lane1,
    B1Lane2,
    B1Lane3,
    B1Lane4,
    B1Lane5,
    B1Lane6,
    B1Lane7,
    B1Lane8,

    B2Lane1,
    B2Lane2,
    B2Lane3,
    B2Lane4,
    B2Lane5,
    B2Lane6,
    B2Lane7,
    B2Lane8,
}
