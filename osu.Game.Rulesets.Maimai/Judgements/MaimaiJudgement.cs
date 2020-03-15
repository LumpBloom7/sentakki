// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Maimai.Judgements
{
    public class MaimaiJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.Perfect;

        protected override int NumericResultFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;

                case HitResult.Ok:
                    return 50;

                case HitResult.Good:
                    return 80;

                case HitResult.Great:
                case HitResult.Perfect:
                    return 100;
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

                case HitResult.Ok:
                case HitResult.Good:
                case HitResult.Great:
                    return 0.2;

                case HitResult.Perfect:
                    return 0.3;
            }
        }
    }
}
