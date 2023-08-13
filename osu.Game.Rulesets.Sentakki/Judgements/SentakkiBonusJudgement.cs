using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Judgements
{
    public class SentakkiBonusJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.LargeBonus;
    }
}
