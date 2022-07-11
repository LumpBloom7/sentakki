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

            SetDefault(SentakkiRulesetSettings.KiaiEffects, true);
            SetDefault(SentakkiRulesetSettings.AnimationDuration, 1000, 100, 2000, 100.0);
            SetDefault(SentakkiRulesetSettings.TouchAnimationDuration, 500, 50, 1000, 50.0);
            SetDefault(SentakkiRulesetSettings.ShowNoteStartIndicators, false);
            SetDefault(SentakkiRulesetSettings.RingColor, ColorOption.Default);
            SetDefault(SentakkiRulesetSettings.RingOpacity, 1f, 0f, 1f, 0.01f);
            SetDefault(SentakkiRulesetSettings.LaneInputMode, LaneInputMode.Button);
            SetDefault(SentakkiRulesetSettings.SnakingSlideBody, true);
            SetDefault(SentakkiRulesetSettings.DetailedJudgements, false);
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
        SnakingSlideBody,
        DetailedJudgements
    }
}
