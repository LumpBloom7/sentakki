using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Sentakki.Configuration;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class SentakkiSettingsSubsection : RulesetSettingsSubsection
    {
        protected override string Header => "sentakki";

        public SentakkiSettingsSubsection(Ruleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (SentakkiRulesetConfigManager)Config;

            // for an odd reason, Config seems to be passed as null when creating it. doesnt even get called...
            if (config == null)
                return;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Use maimai style judgement text (In-game only)",
                    Current = config.GetBindable<bool>(SentakkiRulesetSettings.MaimaiJudgements)
                },
                new SettingsCheckbox
                {
                    LabelText = "Show Kiai effects",
                    Current = config.GetBindable<bool>(SentakkiRulesetSettings.KiaiEffects)
                },
                new SettingsCheckbox
                {
                    LabelText = "Play Break sample when hitting BREAKs perfectly",
                    Current = config.GetBindable<bool>(SentakkiRulesetSettings.BreakSounds)
                },
                new SettingsCheckbox
                {
                    LabelText = "Play Slide sample when beginning to slide",
                    Current = config.GetBindable<bool>(SentakkiRulesetSettings.SlideSounds)
                },
                new SettingsCheckbox
                {
                    LabelText = "Show note start indicators",
                    Current = config.GetBindable<bool>(SentakkiRulesetSettings.ShowNoteStartIndicators)
                },
                new SettingsEnumDropdown<ColorOption>
                {
                    LabelText = "Ring Colour",
                    Current = config.GetBindable<ColorOption>(SentakkiRulesetSettings.RingColor)
                },
                new SettingsSlider<double, NoteTimeSlider>
                {
                    LabelText = "Note entry speed",
                    Current = config.GetBindable<double>(SentakkiRulesetSettings.AnimationDuration),
                },
                new SettingsSlider<double, TouchTimeSlider>
                {
                    LabelText = "Touch note fade-in speed",
                    Current = config.GetBindable<double>(SentakkiRulesetSettings.TouchAnimationDuration),
                },
                new SettingsSlider<float>
                {
                    LabelText = "Ring Opacity",
                    Current = config.GetBindable<float>(SentakkiRulesetSettings.RingOpacity),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsEnumDropdown<LaneInputMode>
                {
                    LabelText = "Lane input mode (Doesn't apply to touch)",
                    Current = config.GetBindable<LaneInputMode>(SentakkiRulesetSettings.LaneInputMode)
                },
            };
        }

        private class NoteTimeSlider : OsuSliderBar<double>
        {
            private string speedRating()
            {
                double speed = (2200 - Current.Value) / 200;

                if (speed == 10.5)
                    return "Sonic";

                return speed.ToString();
            }
            public override string TooltipText => Current.Value.ToString("N0") + "ms (" + speedRating() + ")";
        }
        private class TouchTimeSlider : OsuSliderBar<double>
        {
            private string speedRating()
            {
                double speed = (1100 - Current.Value) / 100;

                if (speed == 10.5)
                    return "Sonic";

                return speed.ToString();
            }
            public override string TooltipText => Current.Value.ToString("N0") + "ms (" + speedRating() + ")";
        }
    }
}
