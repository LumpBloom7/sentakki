using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring
{
    public class SentakkiHitWindows : HitWindows
    {
        public override bool IsHitResultAllowed(HitResult result)
        {
            switch (result)
            {
                case HitResult.Perfect:
                case HitResult.Great:
                case HitResult.Good:
                case HitResult.Miss:
                    return true;
                default:
                    return false;
            }
        }

        protected override DifficultyRange[] GetRanges() => new DifficultyRange[]{
            new DifficultyRange(HitResult.Miss, 144, 144, 72),
            new DifficultyRange(HitResult.Good, 144, 144, 72),
            new DifficultyRange(HitResult.Great, 96, 96, 48),
            new DifficultyRange(HitResult.Perfect, 48, 48, 24),
        };
    }
}
