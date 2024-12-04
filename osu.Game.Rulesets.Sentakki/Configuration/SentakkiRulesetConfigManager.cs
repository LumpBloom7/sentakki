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
            SetDefault(SentakkiRulesetSettings.AnimationSpeed, 2.0f, 1.0f, 10.25f, 0.25f);
            SetDefault(SentakkiRulesetSettings.TouchAnimationSpeed, 2.0f, 1.0f, 10.25f, 0.25f);
            SetDefault(SentakkiRulesetSettings.ShowNoteStartIndicators, false);
            SetDefault(SentakkiRulesetSettings.RingColor, ColorOption.Default);
            SetDefault(SentakkiRulesetSettings.RingOpacity, 1f, 0f, 1f, 0.01f);
            SetDefault(SentakkiRulesetSettings.SnakingSlideBody, true);
            SetDefault(SentakkiRulesetSettings.DetailedJudgements, false);
        }

        public override TrackedSettings CreateTrackedSettings() => new TrackedSettings
        {
            new TrackedSetting<float>(SentakkiRulesetSettings.AnimationSpeed, t => new SettingDescription(
                rawValue: t,
                name: SentakkiSettingsSubsectionStrings.NoteEntrySpeed,
                value: SentakkiSettingsSubsectionStrings.EntrySpeedTooltip(t, DrawableSentakkiRuleset.ComputeLaneNoteEntryTime(t))
            ))
        };
    }

    public enum SentakkiRulesetSettings
    {
        KiaiEffects,
        AnimationSpeed,
        RingOpacity,
        ShowNoteStartIndicators,
        RingColor,
        TouchAnimationSpeed,
        SnakingSlideBody,
        DetailedJudgements,
    }
}
