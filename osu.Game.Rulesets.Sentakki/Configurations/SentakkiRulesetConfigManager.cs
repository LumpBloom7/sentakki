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
            Set(SentakkiRulesetSettings.AnimationDuration, 500, 50.0, 1000, 50.0);
            Set(SentakkiRulesetSettings.MaimaiJudgements, false);
            Set(SentakkiRulesetSettings.ShowNoteStartIndicators, false);
            Set(SentakkiRulesetSettings.DiffBasedRingColor, false);
            Set(SentakkiRulesetSettings.RingOpacity, 1f, 0f, 1f, 0.01f);
        }
    }

    public enum SentakkiRulesetSettings
    {
        KiaiEffects,
        AnimationDuration,
        MaimaiJudgements,
        RingOpacity,
        ShowNoteStartIndicators,
        DiffBasedRingColor
    }
}
