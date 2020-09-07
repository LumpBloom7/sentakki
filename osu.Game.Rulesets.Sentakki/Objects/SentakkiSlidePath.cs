using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SentakkiSlidePath
    {
        // The ending lane of the path, without considering the lane offset of the main body
        public readonly int EndLane;

        public readonly SliderPath Path;

        // The minimum duration that this pattern can have, used in converts
        public readonly double MinDuration;

        // The maximum duration that this pattern can have, used in converts.
        // While it is completely playable even beyond this value, it would look awkward for shorter slides
        public double MaxDuration => MinDuration * 10;

        public SentakkiSlidePath(PathControlPoint[] pathControlPoints, int endLane)
        {
            Path = new SliderPath(pathControlPoints);
            MinDuration = Path.Distance / 2;
            EndLane = endLane;
        }
    }
}
