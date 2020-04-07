using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Maimai.Objects
{
    public class Hold : MaimaiHitObject, IHasEndTime
    {
        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration { get; set; }
    }
}
