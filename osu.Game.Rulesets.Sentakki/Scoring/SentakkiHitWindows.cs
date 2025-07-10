using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Scoring
{
    public abstract class SentakkiHitWindows : HitWindows
    {
        protected const double timing_unit = 1000 / 60.0; // A single frame

        public HitResult MinimumHitResult = HitResult.Miss;

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

        // Sentakki doesn't have variable difficulty
        public override void SetDifficulty(double difficulty) { }

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
                    return result >= MinimumHitResult;

                default:
                    return false;
            }
        }

        public override double WindowFor(HitResult result)
        {
            double window = JudgementMode switch
            {
                SentakkiJudgementMode.Maji => MajiWindowFor(result),
                SentakkiJudgementMode.Gati => GachiWindowFor(result),
                _ => DefaultWindowFor(result),
            };

            return window;
        }

        protected abstract double DefaultWindowFor(HitResult result);
        protected virtual double MajiWindowFor(HitResult result) => DefaultWindowFor(result);
        protected virtual double GachiWindowFor(HitResult result) => MajiWindowFor(result);
    }
}
