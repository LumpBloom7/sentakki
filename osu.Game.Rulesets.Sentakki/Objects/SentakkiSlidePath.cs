using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SentakkiSlidePath
    {
        // The ending lane of the path, without considering the lane offset of the main body
        public int EndLane { get; set; }

        public SliderPath Path { get; set; }

        // The minimum duration that this pattern can have, used in converts
        public double MinDuration => Path.Distance / 2;

        public SentakkiSlidePath(PathControlPoint[] pathControlPoints, int endLane)
        {
            Path = new SliderPath(pathControlPoints);
            EndLane = endLane;
        }
    }
}