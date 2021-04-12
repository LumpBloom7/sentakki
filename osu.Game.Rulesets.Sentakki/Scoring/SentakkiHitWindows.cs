using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring
{
    public class SentakkiHitWindows : HitWindows
    {
        public override bool IsHitResultAllowed(HitResult result)
        {
            switch (result)
            {
                case HitResult.Great:
                case HitResult.Good:
                case HitResult.Ok:
                case HitResult.Miss:
                    return true;
                default:
                    return false;
            }
        }

        protected override DifficultyRange[] GetRanges() => new DifficultyRange[]{
            new DifficultyRange(HitResult.Miss, 144, 144, 72),
            new DifficultyRange(HitResult.Ok, 144, 144, 72),
            new DifficultyRange(HitResult.Good, 96, 96, 48),
            new DifficultyRange(HitResult.Great, 48, 48, 24),
        };
    }
}
