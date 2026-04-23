using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Localisation;

namespace osu.Game.Rulesets.Sentakki.UI;

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

        Children =
        [
            new SettingsItemV2(new FormCheckBox {
                Caption = SentakkiSettingsSubsectionStrings.ShowKiaiEffects,
                Current = config.GetBindable<bool>(SentakkiRulesetSettings.KiaiEffects),
            }) { Keywords = ["visualiser", "visualizer", "bounce"] },

            new SettingsItemV2(new FormCheckBox {
                Caption = SentakkiSettingsSubsectionStrings.ShowNoteStartIndicators,
                Current = config.GetBindable<bool>(SentakkiRulesetSettings.ShowNoteStartIndicators)
            }),

            new SettingsItemV2(new FormCheckBox {
                Caption = SentakkiSettingsSubsectionStrings.SnakingInSlides,
                Current = config.GetBindable<bool>(SentakkiRulesetSettings.SnakingSlideBody)
            }),

            new SettingsItemV2(new FormCheckBox {
                Caption = SentakkiSettingsSubsectionStrings.ShowDetailedJudgements,
                Current = config.GetBindable<bool>(SentakkiRulesetSettings.DetailedJudgements),
            }) { Keywords = ["early", "late","indicators", "timing"] },

            new SettingsItemV2(new FormEnumDropdown<ColorOption> {
               Caption = SentakkiSettingsSubsectionStrings.RingColor,
               Current = config.GetBindable<ColorOption>(SentakkiRulesetSettings.RingColor),
            }) { Keywords = ["color"] },

            new SettingsItemV2(new FormSliderBar<float>{
                Caption = SentakkiSettingsSubsectionStrings.NoteEntrySpeed,
                Current = config.GetBindable<float>(SentakkiRulesetSettings.AnimationSpeed),
                LabelFormat = v => SentakkiSettingsSubsectionStrings.EntrySpeedTooltip(v, DrawableSentakkiRuleset.ComputeLaneNoteEntryTime(v))
            }) { Keywords = ["scroll"] },

            new SettingsItemV2(new FormSliderBar<float>{
                Caption = SentakkiSettingsSubsectionStrings.TouchNoteEntrySpeed,
                Current = config.GetBindable<float>(SentakkiRulesetSettings.TouchAnimationSpeed),
                LabelFormat = v => SentakkiSettingsSubsectionStrings.EntrySpeedTooltip(v, DrawableSentakkiRuleset.ComputeTouchNoteEntryTime(v))
            }) { Keywords = ["scroll"] },

            new SettingsItemV2(new FormSliderBar<float>{
                Caption = SentakkiSettingsSubsectionStrings.RingOpacity,
                Current = config.GetBindable<float>(SentakkiRulesetSettings.RingOpacity),
                DisplayAsPercentage = true,
            }) { Keywords = ["transparency"] },
        ];

        if (!RuntimeInfo.IsMobile)
            return;

        Add(new SettingsItemV2(new FormCheckBox
        {
            Caption = SentakkiSettingsSubsectionStrings.PreferPortraitLayout,
            Current = config.GetBindable<bool>(SentakkiRulesetSettings.PreferPortraitLayout),
        })
        { Keywords = ["override", "orientation"] });
    }
}
