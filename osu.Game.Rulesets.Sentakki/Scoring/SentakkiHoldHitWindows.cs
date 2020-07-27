using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring
{
    // Used as the hitwindows for HOLD tail
    public class SentakkiHoldHitWindows : SentakkiHitWindows
    {
        protected override DifficultyRange[] GetRanges() => new DifficultyRange[]{
            new DifficultyRange(HitResult.Miss, 224, 224, 224),
            new DifficultyRange(HitResult.Meh, 224, 224, 224),
            new DifficultyRange(HitResult.Good, 176, 176, 176),
            new DifficultyRange(HitResult.Great, 64, 64, 64),
            new DifficultyRange(HitResult.Perfect, 16, 16, 16)
        };
    }
}
