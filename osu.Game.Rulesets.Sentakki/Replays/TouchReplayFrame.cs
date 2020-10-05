using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Replays
{
    public class TouchReplayEvent
    {
        public TouchReplayEvent(Vector2 Position, double Duration, double startTime, float rotation = 0)
        {
            MovementPath = new SliderPath(new PathControlPoint[]{
                new PathControlPoint(Position)
            });
            this.Duration = Duration;
            StartTime = startTime;
            Rotation = rotation;
        }

        public TouchReplayEvent(SliderPath path, double Duration, double startTime, float rotation = 0)
        {
            MovementPath = path;
            this.Duration = Duration;
            StartTime = startTime;
            Rotation = rotation;
        }

        public SliderPath MovementPath;
        public double Duration;
        public double StartTime;
        public float Rotation = 0;
    }
}
