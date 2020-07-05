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
                case HitResult.Meh:
                case HitResult.Miss:
                    return true;
                default:
                    return false;
            }
        }
        protected override DifficultyRange[] GetRanges() => new DifficultyRange[]{
            new DifficultyRange(HitResult.Miss, 144,144,144),
            new DifficultyRange(HitResult.Meh, 144, 144, 144 ),
            new DifficultyRange(HitResult.Good, 96, 96 , 96),
            new DifficultyRange(HitResult.Great, 48, 48 , 48),
            new DifficultyRange(HitResult.Perfect, 16, 16 ,16)
        };
    }
}
