using osu.Game.Rulesets.Objects;
using osu.Framework.Utils;
using osuTK;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Replays
{
    public class TouchReplayEvent
    {
        public Vector2 PositionAtTime(double currentTime)
        {
            var completionPercentage = Interpolation.ValueAt(currentTime, 0D, 1D, StartTime, EndTime);

            Vector2 result = MovementPath.PositionAt(completionPercentage);

            // Rotate according to property (used only for autoplay)
            result = result.RotatePointAroundOrigin(Vector2.Zero, Rotation);

            // Offset the final position (again for autoplay)
            result += Offset;

            return result;
        }

        public Vector2 Position
        {
            set => MovementPath = new SliderPath(new[] { new PathControlPoint(value) });
        }

        public SliderPath MovementPath;

        public double StartTime;

        public double Duration { get; set; }

        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public float Rotation = 0;

        // Used to offset autoplay
        public Vector2 Offset = Vector2.Zero;
    }
}
