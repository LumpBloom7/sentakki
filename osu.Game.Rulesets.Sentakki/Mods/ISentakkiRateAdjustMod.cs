using System;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Scoring;

namespace osu.Game.Rulesets.Sentakki.Mods;

/// <summary>
/// May be attached to rate-adjustment mods to adjust hit windows adjust relative to gameplay rate.
/// </summary>
public interface ISentakkiRateAdjustMod : IApplicableToHitObject
{
    BindableNumber<double> SpeedChange { get; }

    void IApplicableToHitObject.ApplyToHitObject(HitObject hitObject)
    {
        switch (hitObject)
        {
            case Tap:
            case Hold:
            case Hold.HoldHead:
            case SlideBody:
            case Touch:
                ((SentakkiHitWindows)hitObject.HitWindows).SpeedMultiplier = SpeedChange.Value;
                break;
        }

        foreach (var nested in hitObject.NestedHitObjects)
            ApplyToHitObject(nested);
    }
}
