using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Judgements
{
    public class SentakkiBreakJudgement : SentakkiJudgement
    {
        public override HitResult MaxResult => HitResult.Perfect;
    }
}
