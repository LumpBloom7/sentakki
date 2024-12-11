﻿using osu.Framework.Allocation;
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
                new SettingsSlider<float, NoteTimeSlider>
                {
                    LabelText = SentakkiSettingsSubsectionStrings.NoteEntrySpeed,
                    Current = config.GetBindable<float>(SentakkiRulesetSettings.AnimationSpeed),
                },
                new SettingsSlider<float, TouchTimeSlider>
                {
                    LabelText = SentakkiSettingsSubsectionStrings.TouchNoteEntrySpeed,
                    Current = config.GetBindable<float>(SentakkiRulesetSettings.TouchAnimationSpeed),
                },
                new SettingsSlider<float>
                {
                    LabelText = SentakkiSettingsSubsectionStrings.RingOpacity,
                    Current = config.GetBindable<float>(SentakkiRulesetSettings.RingOpacity),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
            };
        }

        private partial class NoteTimeSlider : RoundedSliderBar<float>
        {
            public override LocalisableString TooltipText => SentakkiSettingsSubsectionStrings.EntrySpeedTooltip(Current.Value, DrawableSentakkiRuleset.ComputeLaneNoteEntryTime(Current.Value));
        }

        private partial class TouchTimeSlider : RoundedSliderBar<float>
        {
            public override LocalisableString TooltipText => SentakkiSettingsSubsectionStrings.EntrySpeedTooltip(Current.Value, DrawableSentakkiRuleset.ComputeTouchNoteEntryTime(Current.Value));
        }
    }
}
