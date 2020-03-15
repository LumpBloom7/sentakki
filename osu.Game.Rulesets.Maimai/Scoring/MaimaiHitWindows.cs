// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Maimai.Scoring
{
    public class MaimaiHitWindows : HitWindows
    {
        public override bool IsHitResultAllowed(HitResult result)
        {
            switch (result)
            {
                case HitResult.Perfect:
                case HitResult.Great:
                case HitResult.Good:
                case HitResult.Meh:
                case HitResult.Miss:
                    return true;
            }

            return false;
        }

        protected override DifficultyRange[] GetRanges() => new[]
        {
            new DifficultyRange(HitResult.Perfect,0,10,24),
            new DifficultyRange(HitResult.Great, 24, 30, 49),
            new DifficultyRange(HitResult.Good, 49, 50, 69),
            new DifficultyRange(HitResult.Meh, 69, 74, 99),
            new DifficultyRange(HitResult.Miss, 50, 75, 100),
        };
    }
}
