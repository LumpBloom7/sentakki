using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets.Maimai.Configuration
{
    public class MaimaiRulesetConfigManager : RulesetConfigManager<MaimaiRulesetSettings>
    {
        public MaimaiRulesetConfigManager(SettingsStore settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            Set(MaimaiRulesetSettings.KiaiEffects, true);
            Set(MaimaiRulesetSettings.AnimationDuration, 500, 50.0, 1000, 50.0);
            Set(MaimaiRulesetSettings.MaimaiJudgements, false);
            Set(MaimaiRulesetSettings.ShowNoteStartIndicators, false);
            Set(MaimaiRulesetSettings.DiffBasedRingColor, false);
            Set(MaimaiRulesetSettings.RingOpacity, 1f, 0f, 1f, 0.01f);
        }
    }

    public enum MaimaiRulesetSettings
    {
        KiaiEffects,
        AnimationDuration,
        MaimaiJudgements,
        RingOpacity,
        ShowNoteStartIndicators,
        DiffBasedRingColor
    }
}
