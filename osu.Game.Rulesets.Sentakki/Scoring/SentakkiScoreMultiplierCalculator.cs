using System;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Mods;

namespace osu.Game.Rulesets.Sentakki.Scoring;

public class SentakkiScoreMultiplierCalculator : ScoreMultiplierCalculator
{
    public SentakkiScoreMultiplierCalculator(ScoreMultiplierContext context) : base(context)
    {
        #region Difficulty Reduction
        Single<SentakkiModHalfTime>(hasMultiplier: ht => rateAdjustMultiplier(ht.SpeedChange.Value));
        Single<SentakkiModDaycore>(hasMultiplier: dc => rateAdjustMultiplier(dc.SpeedChange.Value));
        #endregion

        #region Difficulty Increase
        Single<SentakkiModHardRock>(hasMultiplier: hr => judgementModeMultiplier(hr.JudgementMode.Value));
        Single<SentakkiModDoubleTime>(hasMultiplier: dt => rateAdjustMultiplier(dt.SpeedChange.Value));
        Single<SentakkiModNightcore>(hasMultiplier: nc => rateAdjustMultiplier(nc.SpeedChange.Value));
        Single<SentakkiModHidden>(hasMultiplier: hd => visibilityRadiusMultiplier(hd.VisibleRadius.Value));
        // Sudden Death
        // Perfect
        // Challenge
        // Accuracy Challenge
        #endregion

        #region Automation
        Single<SentakkiModNoTouch>(hasMultiplier: 0.2);
        // Autoplay
        #endregion

        #region Conversion
        Single<SentakkiModDifficultyAdjust>(hasMultiplier: difficultyAdjustMultiplier);
        // Experimental
        // Classic
        // Mirror
        #endregion

        #region Fun
        Single<ModWindUp>(hasMultiplier: 0.5);
        Single<ModWindDown>(hasMultiplier: 0.5);
        Single<ModAdaptiveSpeed>(hasMultiplier: 0.5);
        Single<SentakkiModSynesthesia>(hasMultiplier: 0.8);
        // Barrel Roll
        // Muted
        #endregion
    }

    private static double rateAdjustMultiplier(double speedChange)
    {
        // Round to the nearest multiple of 0.1.
        double value = (int)(speedChange * 10) / 10.0;

        // Offset back to 0.
        value -= 1;

        if (speedChange >= 1)
            return 1 + value / 5;
        else
            return 0.6 + value;
    }

    private static double judgementModeMultiplier(SentakkiJudgementMode judgementMode)
        => judgementMode switch
        {
            SentakkiJudgementMode.Gati => 1.2,
            SentakkiJudgementMode.Maji => 1.1,
            _ => 1,
        };

    private static double visibilityRadiusMultiplier(float visibilityRadius)
        => 1 + MathF.Pow(1 - visibilityRadius, 2) * 0.2;

    private static double difficultyAdjustMultiplier(SentakkiModDifficultyAdjust modDifficultyAdjust)
    {
        double result = 1;

        if (modDifficultyAdjust.BreakRemoval.Value)
            result *= 0.8;

        if (modDifficultyAdjust.AllEx.Value)
            result *= 0.3;

        return result;
    }
}
