using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets.maimai.Configuration
{
    public class maimaiRulesetConfigManager : RulesetConfigManager<maimaiRulesetSettings>
    {
        public maimaiRulesetConfigManager(SettingsStore settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            Set(maimaiRulesetSettings.ShowVisualizer, true);
        }
    }

    public enum maimaiRulesetSettings
    {
        ShowVisualizer
    }
}
