using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Sentakki.Localisation;

public static class SentakkiEditorStrings
{
    private const string prefix = @"osu.Game.Rulesets.Sentakki.Resources.Localisation.SentakkiEditorStrings";
    private static string getKey(string key) => $"{prefix}:{key}";

    public static LocalisableString TapTool => new TranslatableString(getKey("tap_tool"), @"Tap");
    public static LocalisableString HoldTool => new TranslatableString(getKey("hold_tool"), @"Hold");
    public static LocalisableString SlideTool => new TranslatableString(getKey("slide_tool"), @"Slide");
    public static LocalisableString TouchTool => new TranslatableString(getKey("touch_tool"), @"Touch");
    public static LocalisableString TouchHoldTool => new TranslatableString(getKey("touchhold_tool"), @"TouchHold");

    public static LocalisableString BreakToggle => new TranslatableString(getKey("break_toggle"), @"Break");
    public static LocalisableString BreakToggleTooltip
        => new TranslatableString(
            getKey("break_toggle_tooltip"),
            @"Increases the scoring weight of notes. Typically used to emphasize certain notes, or to increase punishment for inaccuracy."
        );

    public static LocalisableString ExToggle => new TranslatableString(getKey("ex_toggle"), @"Ex");
    public static LocalisableString ExToggleTooltip
        => new TranslatableString(
            getKey("ex_toggle_tooltip"),
            @"Increases the judgement leniency of notes. Typically used to provide a safety net for players, allowing harder patterns to be introduced."
        );

    public static LocalisableString BreakSlideToggle => new TranslatableString(getKey("break_slide_toggle"), @"Break Slide");
    public static LocalisableString ExSlideToggle => new TranslatableString(getKey("ex_slide_toggle"), @"Ex Slide");

    public static LocalisableString LaneSnapGrid => new TranslatableString(getKey("lane_snap_grid"), @"Lane snap grid");
    public static LocalisableString TouchSnapGrid => new TranslatableString(getKey("touch_snap_grid"), @"Touch snap grid");
}
