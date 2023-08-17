using osu.Framework.Configuration.Tracking;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets.Sentakki.Configuration
{
    public class SentakkiRulesetConfigManager : RulesetConfigManager<SentakkiRulesetSettings>
    {
        public SentakkiRulesetConfigManager(SettingsStore? settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            SetDefault(SentakkiRulesetSettings.KiaiEffects, true);
            SetDefault(SentakkiRulesetSettings.AnimationDuration, 1000, 100, 2000, 100.0);
            SetDefault(SentakkiRulesetSettings.TouchAnimationDuration, 500, 50, 1000, 50.0);
            SetDefault(SentakkiRulesetSettings.ShowNoteStartIndicators, false);
            SetDefault(SentakkiRulesetSettings.RingColor, ColorOption.Default);
            SetDefault(SentakkiRulesetSettings.RingOpacity, 1f, 0f, 1f, 0.01f);
            SetDefault(SentakkiRulesetSettings.LaneInputMode, LaneInputMode.Button);
            SetDefault(SentakkiRulesetSettings.SnakingSlideBody, true);
            SetDefault(SentakkiRulesetSettings.DetailedJudgements, false);
            SetDefault(SentakkiRulesetSettings.BreakSampleVolume, 1d, 0d, 1d, 0.01f);
        }

        public override TrackedSettings CreateTrackedSettings() => new TrackedSettings
        {
            new TrackedSetting<double>(SentakkiRulesetSettings.AnimationDuration, t => new SettingDescription(
                rawValue: t,
                name: "Note animation speed",
                value: generateNoteSpeedDescription(t)
            ))
        };

        private string generateNoteSpeedDescription(double time)
        {
            string speedRating()
            {
                double speed = (2200 - time) / 200;

                if (speed == 10.5)
                    return "Sonic";

                return speed.ToString();
            }

            return $"{time:N0}ms ({speedRating()})";
        }
    }

    public enum SentakkiRulesetSettings
    {
        KiaiEffects,
        AnimationDuration,
        RingOpacity,
        ShowNoteStartIndicators,
        RingColor,
        TouchAnimationDuration,
        LaneInputMode,
        SnakingSlideBody,
        DetailedJudgements,
        BreakSampleVolume
    }
}
