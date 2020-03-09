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

            Set(MaimaiRulesetSettings.ShowVisualizer, true);
        }
    }

    public enum MaimaiRulesetSettings
    {
        ShowVisualizer
    }
}
