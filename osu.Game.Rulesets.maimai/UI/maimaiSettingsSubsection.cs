using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.maimai.Configuration;

namespace osu.Game.Rulesets.maimai.UI
{
    class maimaiSettingsSubsection : RulesetSettingsSubsection
    {
        protected override string Header => "maimai";

        public maimaiSettingsSubsection(Ruleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (maimaiRulesetConfigManager)Config;

            // for an odd reason, Config seems to be passed as null when creating it. doesnt even get called...
            if (config == null)
                return;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Show Visualizer",
                    Bindable = config.GetBindable<bool>(maimaiRulesetSettings.ShowVisualizer)
                }
            };
        }
    }
}
