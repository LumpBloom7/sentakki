using System;
using System.Linq;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SentakkiSlidePath
    {
        // The ending lane of the path, without considering the lane offset of the main body
        public readonly int EndLane;

        // The minimum duration that this pattern can have, used in converts
        public double MinDuration => TotalDistance / 2;

        // The maximum duration that this pattern can have, used in converts.
        // While it is completely playable even beyond this value, it would look awkward for shorter slides
        public double MaxDuration => MinDuration * 10;

        public double TotalDistance { get; private set; }

        public readonly SliderPath[] SlideSegments;

        public readonly double FanStartDistance;

        public bool HasFanSlide => FanStartDistance != TotalDistance;

        private Vector2 pathOrigin;

        public SentakkiSlidePath(SliderPath segment, int endLane, bool lastSegmentIsFan)
            : this(new[] { segment }, endLane, lastSegmentIsFan) { }

        public SentakkiSlidePath(SliderPath[] segments, int endLane, bool lastSegmentIsFan)
        {
            TotalDistance = FanStartDistance = segments.Sum(p => p.Distance);
            EndLane = endLane;

            if (lastSegmentIsFan)
            {
                SlideSegments = new SliderPath[segments.Length - 1];
                Array.Copy(segments, SlideSegments, segments.Length - 1);
                FanStartDistance = SlideSegments.Sum(p => p.Distance);
            }
            else
                SlideSegments = segments;

            pathOrigin = segments[0].PositionAt(0);
        }

        public Vector2 PositionAt(double progress, int laneOffset = 0)
        {
            if (progress <= 0) return pathOrigin;
            if (progress >= 1 && FanStartDistance == TotalDistance) return SlideSegments[^1].PositionAt(1);

            double distanceLeft = TotalDistance * progress;
            if (progress < 1)
                foreach (var segment in SlideSegments)
                {
                    if (segment.Distance > distanceLeft)
                        return segment.PositionAt(distanceLeft / segment.Distance);

                    distanceLeft -= segment.Distance;
                }

            // Here starts the fan slide fallback
            var originPos = SlideSegments.Length > 0 ? SlideSegments[^1].PositionAt(1) : pathOrigin;
            float originAngle = Vector2.Zero.GetDegreesFromPosition(originPos);
            float destAngle = originAngle + 180 + (laneOffset * 45);

            var destPosition = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, destAngle);

            return Vector2.Lerp(originPos, destPosition, Math.Clamp((float)(distanceLeft / (TotalDistance - FanStartDistance)), 0, 1));
        }
    }
}
