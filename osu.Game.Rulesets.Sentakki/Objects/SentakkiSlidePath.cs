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

        public readonly float FanStartProgress = 1;

        public bool EndsWithSlideFan { get; private set; }
        public bool StartsWithSlideFan { get; private set; }

        public readonly Vector2 pathOrigin;
        public readonly Vector2 fanOrigin;

        public SentakkiSlidePath(SliderPath segment, int endLane, bool lastSegmentIsFan = false)
            : this(new[] { segment }, endLane, lastSegmentIsFan) { }

        public SentakkiSlidePath(SliderPath[] segments, int endLane, bool lastSegmentIsFan = false)
        {
            fanOrigin = pathOrigin = segments[0].PositionAt(0);
            TotalDistance = segments.Sum(p => p.Distance);
            EndLane = endLane;
            EndsWithSlideFan = lastSegmentIsFan;
            StartsWithSlideFan = lastSegmentIsFan && segments.Length == 1;

            if (lastSegmentIsFan)
            {
                SlideSegments = new SliderPath[segments.Length - 1];
                Array.Copy(segments, SlideSegments, segments.Length - 1);
                FanStartProgress = (float)(SlideSegments.Sum(p => p.Distance) / TotalDistance);
                fanOrigin = segments[^1].PositionAt(0);
            }
            else
                SlideSegments = segments;
        }

        public Vector2 PositionAt(double progress, int laneOffset = 0)
        {
            if (progress <= 0) return pathOrigin;
            if (progress >= 1 && !EndsWithSlideFan) return SlideSegments[^1].PositionAt(1);

            // Handle the regular shapes
            if (progress < FanStartProgress)
            {
                double distanceLeft = TotalDistance * progress;

                foreach (var segment in SlideSegments)
                {
                    if (segment.Distance > distanceLeft)
                        return segment.PositionAt(distanceLeft / segment.Distance);

                    distanceLeft -= segment.Distance;
                }
            }

            // Here starts the fan slide fallback
            float originAngle = Vector2.Zero.GetDegreesFromPosition(fanOrigin);
            float destAngle = originAngle + 180 + (laneOffset * 45);

            var destPosition = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, destAngle);

            return Vector2.Lerp(fanOrigin, destPosition, Math.Clamp((float)((progress - FanStartProgress) / (1 - FanStartProgress)), 0, 1));
        }
    }
}
