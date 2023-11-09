using osu.Framework.Configuration.Tracking;
using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Sentakki.Localisation;
using osu.Game.Rulesets.Sentakki.UI;

namespace osu.Game.Rulesets.Sentakki.Configuration
{
    public class SentakkiRulesetConfigManager : RulesetConfigManager<SentakkiRulesetSettings>
    {
        public SentakkiRulesetConfigManager(SettingsStore? settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            SetDefault(SentakkiRulesetSettings.KiaiEffects, true);
            SetDefault(SentakkiRulesetSettings.AnimationDuration, 2.0f, 1.0f, 10.5f, 0.5f);
            SetDefault(SentakkiRulesetSettings.TouchAnimationDuration, 2.0f, 1.0f, 10.5f, 0.5f);
            SetDefault(SentakkiRulesetSettings.ShowNoteStartIndicators, false);
            SetDefault(SentakkiRulesetSettings.RingColor, ColorOption.Default);
            SetDefault(SentakkiRulesetSettings.RingOpacity, 1f, 0f, 1f, 0.01f);
            SetDefault(SentakkiRulesetSettings.LaneInputMode, LaneInputMode.Button);
            SetDefault(SentakkiRulesetSettings.SnakingSlideBody, true);
            SetDefault(SentakkiRulesetSettings.DetailedJudgements, false);
            SetDefault(SentakkiRulesetSettings.BreakSampleVolume, 1.0, 0.0, 1.0, 0.01);
        }

        public override TrackedSettings CreateTrackedSettings() => new TrackedSettings
        {
            new TrackedSetting<float>(SentakkiRulesetSettings.AnimationDuration, t => new SettingDescription(
                rawValue: t,
                name: SentakkiSettingsSubsectionStrings.NoteEntrySpeed,
                value: SentakkiSettingsSubsectionStrings.EntrySpeedTooltip(t, DrawableSentakkiRuleset.ComputeLaneNoteEntryTime(t))
            ))
        };
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
        DetailedJudgements,
        BreakSampleVolume
    }
}
