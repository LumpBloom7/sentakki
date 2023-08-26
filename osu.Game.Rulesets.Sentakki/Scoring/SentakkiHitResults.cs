using System.ComponentModel;

namespace osu.Game.Rulesets.Sentakki.Scoring;

public enum SentakkiHitResult
{
    Good = 3,
    Great = 4,
    Perfect = 5,
    [Description("Critical Perfect")] Critical = 6,
}
