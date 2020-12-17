using System.ComponentModel;
using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets.Sentakki.Configuration
{
    public class SentakkiRulesetConfigManager : RulesetConfigManager<SentakkiRulesetSettings>
    {
        public SentakkiRulesetConfigManager(SettingsStore settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            Set(SentakkiRulesetSettings.KiaiEffects, true);
            Set(SentakkiRulesetSettings.AnimationDuration, 1000, 100, 2000, 100.0);
            Set(SentakkiRulesetSettings.TouchAnimationDuration, 500, 50, 1000, 50.0);
            Set(SentakkiRulesetSettings.ShowNoteStartIndicators, false);
            Set(SentakkiRulesetSettings.RingColor, ColorOption.Default);
            Set(SentakkiRulesetSettings.RingOpacity, 1f, 0f, 1f, 0.01f);
            Set(SentakkiRulesetSettings.LaneInputMode, LaneInputMode.Button);
            Set(SentakkiRulesetSettings.SnakingSlideBody, true);

        }
    }

    public enum ColorOption
    {
        Default,
        [Description("Difficulty-based color")]
        Difficulty,
        Skin,
    }

    public enum LaneInputMode
    {
        Button,
        Sensor,
    }

    public enum SentakkiRulesetSettings
    {
        KiaiEffects,
        AnimationDuration,
        RingOpacity,
        ShowNoteStartIndicators,
        RingColor,
        TouchAnimationDuration,
        LaneInputMode,
        SnakingSlideBody
    }
}
