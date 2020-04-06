using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Maimai.Configuration;

namespace osu.Game.Rulesets.Maimai.UI
{
    class MaimaiSettingsSubsection : RulesetSettingsSubsection
    {
        protected override string Header => "maimai";

        public MaimaiSettingsSubsection(Ruleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (MaimaiRulesetConfigManager)Config;

            // for an odd reason, Config seems to be passed as null when creating it. doesnt even get called...
            if (config == null)
                return;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Use Maimai style judgement text (In-game only)",
                    Bindable = config.GetBindable<bool>(MaimaiRulesetSettings.MaimaiJudgements)
                },
                new SettingsCheckbox
                {
                    LabelText = "Show Visualizer",
                    Bindable = config.GetBindable<bool>(MaimaiRulesetSettings.ShowVisualizer)
                },
                new SettingsCheckbox
                {
                    LabelText = "Show note start indicators",
                    Bindable = config.GetBindable<bool>(MaimaiRulesetSettings.ShowNoteStartIndicators)
                },
                new SettingsCheckbox
                {
                    LabelText = "Change ring color based on difficulty rating",
                    Bindable = config.GetBindable<bool>(MaimaiRulesetSettings.DiffBasedRingColor)
                },
                new SettingsSlider<double>
                {
                    LabelText = "Note entry animation duration",
                    Bindable = config.GetBindable<double>(MaimaiRulesetSettings.AnimationDuration)
                },
                new SettingsSlider<float>
                {
                    LabelText = "Ring Opacity",
                    Bindable = config.GetBindable<float>(MaimaiRulesetSettings.RingOpacity),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
            };
        }
    }
}
