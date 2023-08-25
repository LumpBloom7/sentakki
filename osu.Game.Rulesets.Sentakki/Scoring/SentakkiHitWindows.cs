using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring
{
    public class SentakkiHitWindows : HitWindows
    {
        protected const double timing_unit = 1000 / 60.0; // A single frame

        public SentakkiHitWindows(SentakkiJudgementMode judgementMode = SentakkiJudgementMode.Normal, HitResult minimumHitResult = HitResult.None)
        {
        }

        public override bool IsHitResultAllowed(HitResult result)
        {
            switch (result)
            {
                case HitResult.Perfect:
                case HitResult.Great:
                case HitResult.Good:
                case HitResult.Ok:
                case HitResult.Miss:
                    return true;

                default:
                    return false;
            }
        }

        protected override DifficultyRange[] GetRanges() => new[]
        {
            SimpleDifficultyRange(HitResult.Miss, 9 * timing_unit),
            SimpleDifficultyRange(HitResult.Ok, 9 * timing_unit),
            SimpleDifficultyRange(HitResult.Good, 6 * timing_unit),
            SimpleDifficultyRange(HitResult.Great, 3 * timing_unit),
            SimpleDifficultyRange(HitResult.Perfect, 1 * timing_unit),
        };

        protected static DifficultyRange SimpleDifficultyRange(HitResult result, double range)
            => new DifficultyRange(result, range, range, range);
    }
}
