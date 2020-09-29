using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Judgements
{
    public class SentakkiJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.Perfect;
    }
}
