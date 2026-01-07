using System.ComponentModel;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Localisation.Mods;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Scoring;

namespace osu.Game.Rulesets.Sentakki.Mods;

public class SentakkiModHardRock : ModHardRock, IApplicableToHitObject, IApplicableToDrawableHitObject
{
    public override double ScoreMultiplier
    {
        get
        {
            switch (JudgementMode.Value)
            {
                case SentakkiJudgementMode.Gati:
                    return 1.2;

                case SentakkiJudgementMode.Maji:
                    return 1.1;

                default:
                    return 1;
            }
        }
    }

    public override string ExtendedIconInformation => $"{(JudgementMode.Value == SentakkiJudgementMode.Normal ? string.Empty : JudgementMode.Value)}";

    public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
    {
        // This is a no-op since we don't use beatmap difficulty
        // The only reason we still inherit from ModHardRock is to be able to use their localized strings
    }

    [SettingSource(typeof(SentakkiModHardRockStrings), nameof(SentakkiModHardRockStrings.JudgementMode), nameof(SentakkiModHardRockStrings.JudgementModeDescription))]
    public Bindable<SentakkiJudgementMode> JudgementMode { get; } = new Bindable<SentakkiJudgementMode>(SentakkiJudgementMode.Maji);

    [SettingSource(typeof(SentakkiModHardRockStrings), nameof(SentakkiModHardRockStrings.MinimumResult), nameof(SentakkiModHardRockStrings.MinimumResultDescription))]
    public Bindable<SentakkiHitResult> MinimumValidResult { get; } = new Bindable<SentakkiHitResult>(SentakkiHitResult.Good);

    [SettingSource("Enable strict slider tracking")]
    public Bindable<bool> StrictSliderTracking { get; } = new Bindable<bool>();

    public void ApplyToHitObject(HitObject hitObject)
    {
        // Nested HitObjects should get the same treatment
        foreach (var nested in hitObject.NestedHitObjects)
            ApplyToHitObject(nested);

        if (hitObject.HitWindows is not SentakkiHitWindows shw)
            return;

        shw.MinimumHitResult = (HitResult)MinimumValidResult.Value;
        shw.JudgementMode = JudgementMode.Value;
    }

    public void ApplyToDrawableHitObject(DrawableHitObject drawable)
    {
        if (drawable is not DrawableSlideCheckpoint slideCheckpoint)
            return;

        slideCheckpoint.StrictSliderTracking = StrictSliderTracking.Value;
    }

    public enum SentakkiHitResult
    {
        Good = 2,
        Great = 4,
        Perfect = 5,

        [Description("Critical Perfect")]
        Critical = 6,
    }
}
