using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Judgements;

public class SentakkiJudgementResult : JudgementResult
{
    public SentakkiJudgementResult(HitObject hitObject, Judgement judgement) : base(hitObject, judgement)
    {
    }

    public new HitResult Type
    {
        get => base.Type;
        set
        {
            Critical = value == HitResult.Perfect;
            base.Type = Critical ? HitResult.Great : value;
        }
    }

    public bool Critical { get; set; }
}
