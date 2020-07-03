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
        private static Vector2 getPositionInBetween(Vector2 first, Vector2 second, float ratio = .5f) => first + ((second - first) * ratio);

        // Covers DX Straight 3-7
        public static List<PathControlPoint> GenerateStraightPattern(int start, int end)
        {
            // Cleanup values
            start = start.NormalizePath();
            end = end.NormalizePath();

            return new List<PathControlPoint>{
                new PathControlPoint(SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start)+ new Vector2(300), PathType.Linear),
                new PathControlPoint(getPositionInBetween(SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start),SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, end))+ new Vector2(300), PathType.Linear),
                new PathControlPoint(SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, end)+ new Vector2(300), PathType.Linear),
            };
        }

        // Thunder pattern
        public static List<PathControlPoint> GenerateThunderPattern(int start)
        {
            start = start.NormalizePath();
            Vector2 Node0Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start) + new Vector2(300);
            Vector2 Node1Pos = getPositionInBetween(Node0Pos, SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start + 5) + new Vector2(300), .57f);
            Vector2 Node3Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start + 4) + new Vector2(300);
            Vector2 Node2Pos = getPositionInBetween(Node3Pos, SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start + 1) + new Vector2(300), .57f);

            return new List<PathControlPoint>{
                new PathControlPoint(Node0Pos, PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.Linear),
                new PathControlPoint(new Vector2(300), PathType.Linear),
                new PathControlPoint(Node2Pos, PathType.Linear),
                new PathControlPoint(Node3Pos, PathType.Linear)
            };
        }

        // Covers DX V pattern 1-8
        public static List<PathControlPoint> GenerateVPattern(int start, int end)
        {
            start = start.NormalizePath();
            end = end.NormalizePath();
            end = Math.Clamp(end, start + 2, start + 6).NormalizePath();

            Vector2 Node0Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start) + new Vector2(300);
            Vector2 Node1Pos = new Vector2(300);
            Vector2 Node2Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, end) + new Vector2(300);

            return new List<PathControlPoint>{
                new PathControlPoint(Node0Pos, PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.Linear),
                new PathControlPoint(Node2Pos, PathType.Linear)
            };
        }

        // Covers DX L pattern 2-5
        public static List<PathControlPoint> GenerateLPattern(int start, int end)
        {
            start = start.NormalizePath();
            end = end.NormalizePath();

            Vector2 Node0Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start) + new Vector2(300);
            Vector2 Node1Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start + 6) + new Vector2(300);
            Vector2 Node2Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, end) + new Vector2(300);

            return new List<PathControlPoint>{
                new PathControlPoint(Node0Pos, PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.Linear),
                new PathControlPoint(Node2Pos, PathType.Linear)
            };
        }

        // DX Circle Pattern
        public static List<PathControlPoint> GenerateCirclePattern(int start, int end, int rotation = +1)
        {
            start = start.NormalizePath();

            List<PathControlPoint> SlidePath = new List<PathControlPoint> { };
            if (rotation >= 0)
                for (int i = start; i <= end; ++i)
                    SlidePath.Add(new PathControlPoint(SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, i.NormalizePath()) + new Vector2(300)));
            else
                for (int i = end; i >= start; --i)
                    SlidePath.Add(new PathControlPoint(SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, i.NormalizePath()) + new Vector2(300)));

            SlidePath[0].Type.Value = PathType.Catmull;
            return SlidePath;
        }

        public static List<PathControlPoint> GenerateUPattern(int start, int end)
        {
            start = start.NormalizePath();
            end = end.NormalizePath();

            Vector2 Node0Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start) + new Vector2(300);
            Vector2 Node1Pos = getPositionInBetween(Node0Pos, SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start + 5) + new Vector2(300));
            Vector2 Node2Pos = new Vector2(250, 400);
            Vector2 Node4Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, end) + new Vector2(300);
            Vector2 Node3Pos = getPositionInBetween(Node4Pos, SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, end + 3) + new Vector2(300));

            return new List<PathControlPoint>{
                new PathControlPoint(Node0Pos,PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.PerfectCurve),
                new PathControlPoint(Node2Pos),
                new PathControlPoint(Node3Pos, PathType.PerfectCurve),
                new PathControlPoint(Node4Pos, PathType.Linear)
            };
        }
    }
}