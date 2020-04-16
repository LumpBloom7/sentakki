using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Maimai.Objects
{
    public class TouchHold : MaimaiHitObject, IHasEndTime
    {
        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration { get; set; }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
