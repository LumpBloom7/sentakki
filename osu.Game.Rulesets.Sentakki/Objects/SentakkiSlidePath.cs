using System.Linq;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SentakkiSlidePath
    {
        // The ending lane of the path, without considering the lane offset of the main body
        public readonly int EndLane;

        public readonly SliderPath Path;

        // The minimum duration that this pattern can have, used in converts
        public double MinDuration => TotalDistance / 2;

        // The maximum duration that this pattern can have, used in converts.
        // While it is completely playable even beyond this value, it would look awkward for shorter slides
        public double MaxDuration => MinDuration * 10;

        public double TotalDistance;

        public readonly SliderPath[] SlideSegments;

        public SentakkiSlidePath(SliderPath segment, int endLane)
        {
            SlideSegments = new SliderPath[] { segment };
            TotalDistance = SlideSegments.Sum(p => p.Distance);
            EndLane = endLane;
        }

        public SentakkiSlidePath(SliderPath[] segments, int endLane)
        {
            SlideSegments = segments;
            TotalDistance = SlideSegments.Sum(p => p.Distance);
            EndLane = endLane;
        }
    }
}
