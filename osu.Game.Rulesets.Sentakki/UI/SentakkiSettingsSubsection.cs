using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Localisation;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public partial class SentakkiSettingsSubsection : RulesetSettingsSubsection
    {
        private readonly Ruleset ruleset;

        protected override LocalisableString Header => ruleset.Description;

        public SentakkiSettingsSubsection(Ruleset ruleset)
            : base(ruleset)
        {
            this.ruleset = ruleset;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (SentakkiRulesetConfigManager)Config;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = SentakkiSettingsSubsectionStrings.ShowKiaiEffects,
                    Current = config.GetBindable<bool>(SentakkiRulesetSettings.KiaiEffects)
                },
                new SettingsCheckbox
                {
                    LabelText = SentakkiSettingsSubsectionStrings.ShowNoteStartIndicators,
                    Current = config.GetBindable<bool>(SentakkiRulesetSettings.ShowNoteStartIndicators)
                },
                new SettingsCheckbox
                {
                    LabelText = SentakkiSettingsSubsectionStrings.SnakingInSlides,
                    Current = config.GetBindable<bool>(SentakkiRulesetSettings.SnakingSlideBody)
                },
                new SettingsCheckbox
                {
                    LabelText = SentakkiSettingsSubsectionStrings.ShowDetailedJudgements,
                    Current = config.GetBindable<bool>(SentakkiRulesetSettings.DetailedJudgements)
                },
                new SettingsEnumDropdown<ColorOption>
                {
                    LabelText = SentakkiSettingsSubsectionStrings.RingColor,
                    Current = config.GetBindable<ColorOption>(SentakkiRulesetSettings.RingColor)
                },
                new SettingsSlider<double, NoteTimeSlider>
                {
                    LabelText = SentakkiSettingsSubsectionStrings.NoteEntrySpeed,
                    Current = config.GetBindable<double>(SentakkiRulesetSettings.AnimationDuration),
                },
                new SettingsSlider<double, TouchTimeSlider>
                {
                    LabelText = SentakkiSettingsSubsectionStrings.TouchNoteFadeInSpeed,
                    Current = config.GetBindable<double>(SentakkiRulesetSettings.TouchAnimationDuration),
                },
                new SettingsSlider<float>
                {
                    LabelText = SentakkiSettingsSubsectionStrings.RingOpacity,
                    Current = config.GetBindable<float>(SentakkiRulesetSettings.RingOpacity),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsEnumDropdown<LaneInputMode>
                {
                    LabelText = SentakkiSettingsSubsectionStrings.LaneInputMode,
                    Current = config.GetBindable<LaneInputMode>(SentakkiRulesetSettings.LaneInputMode)
                },
                new SettingsSlider<double, RoundedSliderBar<double>>
                {
                    LabelText = SentakkiSettingsSubsectionStrings.BreakSampleVolume,
                    Current = config.GetBindable<double>(SentakkiRulesetSettings.BreakSampleVolume),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true,
                }
            };
        }

        private partial class NoteTimeSlider : RoundedSliderBar<double>
        {
            private string speedRating()
            {
                double speed = (2200 - Current.Value) / 200;

                if (speed == 10.5)
                    return "Sonic";

                return speed.ToString();
            }

            public override LocalisableString TooltipText => Current.Value.ToString("N0") + "ms (" + speedRating() + ")";
        }

        private partial class TouchTimeSlider : RoundedSliderBar<double>
        {
            private string speedRating()
            {
                double speed = (1100 - Current.Value) / 100;

                if (speed == 10.5)
                    return "Sonic";

                return speed.ToString();
            }

            public override LocalisableString TooltipText => Current.Value.ToString("N0") + "ms (" + speedRating() + ")";
        }
    }
}
