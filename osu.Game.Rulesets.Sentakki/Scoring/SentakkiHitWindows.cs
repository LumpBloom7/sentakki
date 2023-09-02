using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring
{
    public abstract class SentakkiHitWindows : HitWindows
    {
        protected const double timing_unit = 1000 / 60.0; // A single frame

        public HitResult MinimumHitResult = HitResult.None;

        private SentakkiJudgementMode judgementMode = SentakkiJudgementMode.Normal;
        public SentakkiJudgementMode JudgementMode
        {
            get => judgementMode;
            set
            {
                if (value == judgementMode)
                    return;

                judgementMode = value;
                SetDifficulty(0);
            }
        }

        public override bool IsHitResultAllowed(HitResult result)
        {
            switch (result)
            {
                // These are guaranteed to be valid
                case HitResult.Perfect:
                case HitResult.Miss:
                    return true;

                // These are conditional on the minimum valid result
                case HitResult.Great:
                case HitResult.Good:
                case HitResult.Ok:
                    if (result < MinimumHitResult)
                        return false;

                    return true;

                default:
                    return false;
            }
        }

        protected abstract DifficultyRange[] GetDefaultRanges();
        protected virtual DifficultyRange[] GetMajiRanges() => GetDefaultRanges();
        protected virtual DifficultyRange[] GetGachiRanges() => GetMajiRanges();

        protected sealed override DifficultyRange[] GetRanges() => JudgementMode switch
        {
            SentakkiJudgementMode.Maji => GetMajiRanges(),
            SentakkiJudgementMode.Gati => GetGachiRanges(),
            _ => GetDefaultRanges(),
        };

        protected static DifficultyRange SimpleDifficultyRange(HitResult result, double range)
            => new DifficultyRange(result, range, range, range);
    }
}
