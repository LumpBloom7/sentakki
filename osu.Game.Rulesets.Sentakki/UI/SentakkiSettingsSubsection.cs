﻿using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Sentakki.Configuration;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class SentakkiSettingsSubsection : RulesetSettingsSubsection
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

            // for an odd reason, Config seems to be passed as null when creating it. doesnt even get called...
            if (config == null)
                return;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Show Kiai effects",
                    Current = config.GetBindable<bool>(SentakkiRulesetSettings.KiaiEffects)
                },
                new SettingsCheckbox
                {
                    LabelText = "Show note start indicators",
                    Current = config.GetBindable<bool>(SentakkiRulesetSettings.ShowNoteStartIndicators)
                },
                new SettingsCheckbox
                {
                    LabelText = "Snaking in Slides",
                    Current = config.GetBindable<bool>(SentakkiRulesetSettings.SnakingSlideBody)
                },
                new SettingsCheckbox{
                    LabelText = "Show detailed judgements",
                    Current = config.GetBindable<bool>(SentakkiRulesetSettings.DetailedJudgements)
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
                new SettingsCheckbox{
                    LabelText = "Broadcast gameplay events",
                    TooltipText = "Allows third party programs to receive events. (RGB lights or whatnot)",
                    Current = config.GetBindable<bool>(SentakkiRulesetSettings.GameplayIPC)
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
            public override LocalisableString TooltipText => Current.Value.ToString("N0") + "ms (" + speedRating() + ")";
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
            public override LocalisableString TooltipText => Current.Value.ToString("N0") + "ms (" + speedRating() + ")";
        }
    }
}
