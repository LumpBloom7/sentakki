using System;
using System.Collections.Generic;
using osuTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.UI;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public static class SlidePaths
    {
        public static SentakkiSlidePath[] ValidPaths => new SentakkiSlidePath[]{
            GenerateCirclePattern(0),
            GenerateCirclePattern(1),
            GenerateCirclePattern(2),
            GenerateCirclePattern(3),
            GenerateCirclePattern(4),
            GenerateCirclePattern(5),
            GenerateCirclePattern(6),
            GenerateCirclePattern(7),
            GenerateCirclePattern(0, RotationDirection.CounterClockwise),
            GenerateCirclePattern(1, RotationDirection.CounterClockwise),
            GenerateCirclePattern(2, RotationDirection.CounterClockwise),
            GenerateCirclePattern(3, RotationDirection.CounterClockwise),
            GenerateCirclePattern(4, RotationDirection.CounterClockwise),
            GenerateCirclePattern(5, RotationDirection.CounterClockwise),
            GenerateCirclePattern(6, RotationDirection.CounterClockwise),
            GenerateCirclePattern(7, RotationDirection.CounterClockwise),
            GenerateLPattern(1),
            GenerateLPattern(2),
            GenerateLPattern(3),
            GenerateLPattern(4),
            GenerateStraightPattern(2),
            GenerateStraightPattern(3),
            GenerateStraightPattern(4),
            GenerateStraightPattern(5),
            GenerateStraightPattern(6),
            GenerateThunderPattern(),
            GenerateUPattern(0),
            GenerateUPattern(1),
            GenerateUPattern(2),
            GenerateUPattern(3),
            GenerateUPattern(4),
            GenerateUPattern(5),
            GenerateUPattern(6),
            GenerateUPattern(7),
            GenerateVPattern(1),
            GenerateVPattern(2),
            GenerateVPattern(3),
            GenerateVPattern(4),
            GenerateVPattern(5),
            GenerateVPattern(6),
            GenerateVPattern(7),
        };
        private static Vector2 getPositionInBetween(Vector2 first, Vector2 second, float ratio = .5f) => first + ((second - first) * ratio);

        // Covers DX Straight 3-7
        public static SentakkiSlidePath GenerateStraightPattern(int end)
        {
            var controlPoints = new List<PathControlPoint>{
                new PathControlPoint(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0), PathType.Linear),
                new PathControlPoint(getPositionInBetween(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0),SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end)), PathType.Linear),
                new PathControlPoint(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end), PathType.Linear),
            }.ToArray();

            return new SentakkiSlidePath(controlPoints, end);
        }

        private static Vector2 getIntesectPoint(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2)
        {
            float tmp = ((B2.X - B1.X) * (A2.Y - A1.Y)) - ((B2.Y - B1.Y) * (A2.X - A1.X));
            float mu = (((A1.X - B1.X) * (A2.Y - A1.Y)) - ((A1.Y - B1.Y) * (A2.X - A1.X))) / tmp;

            return new Vector2(
                B1.X + ((B2.X - B1.X) * mu),
                B1.Y + ((B2.Y - B1.Y) * mu)
            );
        }

        // Thunder pattern
        public static SentakkiSlidePath GenerateThunderPattern(bool mirrored = false)
        {
            int lane1 = mirrored ? 3 : 5;
            int lane2 = mirrored ? 2 : 6;
            int lane3 = mirrored ? 6 : 2;
            int lane4 = mirrored ? 7 : 1;
            static Vector2 lanestart(int x) => SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, x);
            Vector2 Node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0);
            Vector2 Node1Pos = getIntesectPoint(lanestart(0), lanestart(lane1), lanestart(lane2), lanestart(lane3));

            Vector2 Node2Pos = getIntesectPoint(lanestart(lane2), lanestart(lane3), lanestart(lane4), lanestart(4));
            Vector2 Node3Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 4);

            var controlPoints = new List<PathControlPoint>{
                new PathControlPoint(Node0Pos, PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.Linear),
                new PathControlPoint(Node2Pos, PathType.Linear),
                new PathControlPoint(Node3Pos, PathType.Linear)
            };

            return new SentakkiSlidePath(controlPoints.ToArray(), 4);
        }

        // Covers DX V pattern 1-8
        public static SentakkiSlidePath GenerateVPattern(int end)
        {
            Vector2 Node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0);
            Vector2 Node1Pos = Vector2.Zero;
            Vector2 Node2Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end);

            var controlPoints = new List<PathControlPoint>{
                new PathControlPoint(Node0Pos, PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.Linear),
                new PathControlPoint(Node2Pos, PathType.Linear)
            }.ToArray();

            return new SentakkiSlidePath(controlPoints, end);
        }

        // Covers DX L pattern 2-5
        public static SentakkiSlidePath GenerateLPattern(int end, bool mirrored = false)
        {
            Vector2 Node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0);
            Vector2 Node1Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, mirrored ? 2 : 6);
            Vector2 Node2Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end);

            var controlPoints = new List<PathControlPoint>{
                new PathControlPoint(Node0Pos, PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.Linear),
                new PathControlPoint(Node2Pos, PathType.Linear)
            }.ToArray();

            return new SentakkiSlidePath(controlPoints, end);
        }

        // DX Circle Pattern
        public static SentakkiSlidePath GenerateCirclePattern(int end, RotationDirection direction = RotationDirection.Clockwise)
        {
            float centre = ((0.GetRotationForLane() + end.GetRotationForLane()) / 2) + (direction == RotationDirection.CounterClockwise ? 180 : 0);
            Vector2 centreNode = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, centre == 0.GetRotationForLane() ? centre + 180 : centre);

            List<PathControlPoint> SlidePath = new List<PathControlPoint> {
                new PathControlPoint(SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, 0.GetRotationForLane() + (direction == RotationDirection.CounterClockwise ? -.5f : .5f)), PathType.PerfectCurve),
                new PathControlPoint(centreNode),
                new PathControlPoint(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end), PathType.PerfectCurve)
            };

            return new SentakkiSlidePath(SlidePath.ToArray(), end);
        }

        public static SentakkiSlidePath GenerateUPattern(int end)
        {
            Vector2 Node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0);
            Vector2 Node1Pos = getPositionInBetween(Node0Pos, SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 5), .51f);

            float angleDiff = ((end.GetRotationForLane() + 0.GetRotationForLane()) / 2) + (Math.Abs(end) > 4 ? 0 : 180);
            Vector2 Node2Pos = SentakkiExtensions.GetCircularPosition(115, angleDiff);

            Vector2 Node4Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end);
            Vector2 Node3Pos = getPositionInBetween(Node4Pos, SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + 3), .51f);

            var controlPoints = new List<PathControlPoint>{
                new PathControlPoint(Node0Pos,PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.PerfectCurve),
                new PathControlPoint(Node2Pos),
                new PathControlPoint(Node3Pos, PathType.Linear),
                new PathControlPoint(Node4Pos)
            }.ToArray();

            return new SentakkiSlidePath(controlPoints, end);
        }
    }
}