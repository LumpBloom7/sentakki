using System.ComponentModel;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Localisation.Mods;
using osu.Game.Rulesets.Sentakki.Scoring;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModHardRock : ModHardRock, IApplicableToHitObject
    {
        public override double ScoreMultiplier => 1;

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            // This is a no-op since we don't use beatmap difficulty
            // The only reason we still inherit from ModHardRock is to be able to use their localized strings
        }

        [SettingSource(typeof(SentakkiModHardRockStrings), nameof(SentakkiModHardRockStrings.JudgementMode), nameof(SentakkiModHardRockStrings.JudgementModeDescription))]
        public Bindable<SentakkiJudgementMode> judgementMode { get; } = new Bindable<SentakkiJudgementMode>(SentakkiJudgementMode.Maji);

        [SettingSource(typeof(SentakkiModHardRockStrings), nameof(SentakkiModHardRockStrings.MinimumResult), nameof(SentakkiModHardRockStrings.MinimumResultDescription))]
        public Bindable<SentakkiHitResult> minimumValidResult { get; } = new Bindable<SentakkiHitResult>(SentakkiHitResult.Good);

        public void ApplyToHitObject(HitObject hitObject)
        {
            // Nested HitObjects should get the same treatment
            foreach (var nested in hitObject.NestedHitObjects)
                ApplyToHitObject(nested);

            if (hitObject.HitWindows is not SentakkiHitWindows shw)
                return;

            shw.MinimumHitResult = (HitResult)minimumValidResult.Value;
            shw.JudgementMode = judgementMode.Value;
        }

        public enum SentakkiHitResult
        {
            Good = 3,
            Great = 4,
            Perfect = 5,
            [Description("Critical Perfect")] Critical = 6,
        }
    }
}
