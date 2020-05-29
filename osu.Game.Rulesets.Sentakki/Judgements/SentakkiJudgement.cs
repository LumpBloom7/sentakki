using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Judgements
{
    public class SentakkiJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.Perfect;

        protected override int NumericResultFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;

                case HitResult.Meh:
                    return 250;

                case HitResult.Good:
                    return 400;

                case HitResult.Great:
                case HitResult.Perfect:
                    return 500;
            }
        }

        protected override double HealthIncreaseFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;

                case HitResult.Miss:
                    return -0.1;

                case HitResult.Meh:
                case HitResult.Good:
                case HitResult.Great:
                    return 0.2;

                case HitResult.Perfect:
                    return 0.3;
            }
        }
    }
}
