using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SentakkiSlidePath
    {
        // The ending lane of the path, without considering the lane offset of the main body
        public int EndLane { get; set; }

        public SliderPath Path { get; set; }

        public SentakkiSlidePath(PathControlPoint[] pathControlPoints, int endLane)
        {
            Path = new SliderPath(pathControlPoints);
            EndLane = endLane;
        }
    }
}