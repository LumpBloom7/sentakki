using System;
using System.Collections.Generic;
using osuTK;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.UI;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public static class SlidePaths
    {
        public static List<PathControlPoint>[] ValidPaths => new List<PathControlPoint>[]{
            GenerateCirclePattern(0),
            GenerateCirclePattern(1),
            GenerateCirclePattern(2),
            GenerateCirclePattern(3),
            GenerateCirclePattern(4),
            GenerateCirclePattern(5),
            GenerateCirclePattern(6),
            GenerateCirclePattern(7),
            /* GenerateCirclePattern(0,-1),
            GenerateCirclePattern(1,-1),
            GenerateCirclePattern(2,-1),
            GenerateCirclePattern(3,-1),
            GenerateCirclePattern(4,-1),
            GenerateCirclePattern(5,-1),
            GenerateCirclePattern(6,-1),
            GenerateCirclePattern(7,-1), */
            GenerateLPattern(2),
            GenerateLPattern(3),
            GenerateLPattern(4),
            GenerateLPattern(5),
            GenerateStraightPattern(3),
            GenerateStraightPattern(4),
            GenerateStraightPattern(5),
            GenerateStraightPattern(6),
            GenerateStraightPattern(7),
            GenerateThunderPattern(),
            GenerateUPattern(0),
            GenerateUPattern(1),
            GenerateUPattern(2),
            GenerateUPattern(3),
            GenerateUPattern(4),
            GenerateUPattern(5),
            GenerateUPattern(6),
            GenerateUPattern(7),
            GenerateVPattern(0),
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
        public static List<PathControlPoint> GenerateStraightPattern(int end)
        {
            return new List<PathControlPoint>{
                new PathControlPoint(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0), PathType.Linear),
                new PathControlPoint(getPositionInBetween(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0),SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end)), PathType.Linear),
                new PathControlPoint(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end), PathType.Linear),
            };
        }

        // Thunder pattern
        public static List<PathControlPoint> GenerateThunderPattern()
        {
            Vector2 Node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0);
            Vector2 Node1Pos = getPositionInBetween(Node0Pos, SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 5), .57f);
            Vector2 Node3Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 4);
            Vector2 Node2Pos = getPositionInBetween(Node3Pos, SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 1), .57f);

            return new List<PathControlPoint>{
                new PathControlPoint(Node0Pos, PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.Linear),
                new PathControlPoint(Vector2.Zero, PathType.Linear),
                new PathControlPoint(Node2Pos, PathType.Linear),
                new PathControlPoint(Node3Pos, PathType.Linear)
            };
        }

        // Covers DX V pattern 1-8
        public static List<PathControlPoint> GenerateVPattern(int end)
        {
            Vector2 Node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0);
            Vector2 Node1Pos = Vector2.Zero;
            Vector2 Node2Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end);

            return new List<PathControlPoint>{
                new PathControlPoint(Node0Pos, PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.Linear),
                new PathControlPoint(Node2Pos, PathType.Linear)
            };
        }

        // Covers DX L pattern 2-5
        public static List<PathControlPoint> GenerateLPattern(int end)
        {
            Vector2 Node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0);
            Vector2 Node1Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 6);
            Vector2 Node2Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end);

            return new List<PathControlPoint>{
                new PathControlPoint(Node0Pos, PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.Linear),
                new PathControlPoint(Node2Pos, PathType.Linear)
            };
        }

        // DX Circle Pattern
        public static List<PathControlPoint> GenerateCirclePattern(int end, int rotation = +1)
        {
            float centre = (0.GetRotationForLane() + end.GetRotationForLane()) / 2;
            Vector2 centreNode = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, centre == 0.GetRotationForLane() ? centre + 180 : centre);

            List<PathControlPoint> SlidePath = new List<PathControlPoint> {
                new PathControlPoint(SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, 0.GetRotationForLane()+.5f), PathType.PerfectCurve),
                new PathControlPoint(centreNode),
                new PathControlPoint(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end), PathType.PerfectCurve)
            };
            if (rotation < 0) SlidePath.Reverse();
            return SlidePath;
        }

        public static List<PathControlPoint> GenerateUPattern(int end)
        {
            Vector2 Node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0);
            Vector2 Node1Pos = getPositionInBetween(Node0Pos, SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 5), .51f);

            float angleDiff = (end.GetRotationForLane() + 0.GetRotationForLane()) / 2 + (Math.Abs(end) > 4 ? 0 : 180);
            Vector2 Node2Pos = SentakkiExtensions.GetCircularPosition(115, angleDiff);

            Vector2 Node4Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end);
            Vector2 Node3Pos = getPositionInBetween(Node4Pos, SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + 3), .51f);

            return new List<PathControlPoint>{
                new PathControlPoint(Node0Pos,PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.PerfectCurve),
                new PathControlPoint(Node2Pos),
                new PathControlPoint(Node3Pos, PathType.Linear),
                new PathControlPoint(Node4Pos)
            };
        }
    }
}