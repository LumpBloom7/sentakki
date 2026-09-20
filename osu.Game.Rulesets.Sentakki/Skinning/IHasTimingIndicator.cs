using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Sentakki.Skinning;

/// <summary>
/// Common interface to expose functionality to apply timing indicators to judgements
/// </summary>
public interface IHasTimingIndicator
{
    void ApplyTimingIndicatorFor(JudgementResult judgementResult);
}
