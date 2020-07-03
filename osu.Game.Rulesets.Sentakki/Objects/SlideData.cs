using System;
using System.Collections.Generic;
using osuTK;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.UI;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SlideNode
    {
        public SlideNode(PathType type, Vector2 position)
        {
            Type = type;
            Position = position;
        }
        public readonly PathType Type;
        public readonly Vector2 Position;
    }
    public static class SlidePaths
    {
        private static Vector2 getPositionInBetween(Vector2 first, Vector2 second, float ratio = .5f) => first + ((second - first) * ratio);

        // Covers DX Straight 3-7
        public static SlideNode[] GenerateStraightPattern(int start, int end)
        {
            // Cleanup values
            start = start.NormalizePath();
            end = end.NormalizePath();
            end = Math.Clamp(end, start + 2, start + 6).NormalizePath();

            return new SlideNode[]{
                new SlideNode(PathType.Linear, SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start)),
                new SlideNode(PathType.Linear, getPositionInBetween(SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start),SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, end))),
                new SlideNode(PathType.Linear, SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, end)),
            };
        }

        // Thunder pattern
        public static SlideNode[] GenerateThunderPattern(int start)
        {
            start = start.NormalizePath();
            Vector2 Node0Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start);
            Vector2 Node1Pos = getPositionInBetween(Node0Pos, SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start + 5));
            Vector2 Node2Pos = getPositionInBetween(Node1Pos, SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start + 4));
            Vector2 Node3Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start + 4);

            return new SlideNode[]{
                new SlideNode(PathType.Linear, Node0Pos),
                new SlideNode(PathType.Linear, Node1Pos),
                new SlideNode(PathType.Linear, Node2Pos),
                new SlideNode(PathType.Linear, Node3Pos)
            };
        }

        // Covers DX V pattern 1-8
        public static SlideNode[] GenerateVPattern(int start, int end)
        {
            start = start.NormalizePath();
            end = end.NormalizePath();
            end = Math.Clamp(end, start + 2, start + 6).NormalizePath();

            Vector2 Node0Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start);
            Vector2 Node1Pos = Vector2.Zero;
            Vector2 Node2Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, end);

            return new SlideNode[]{
                new SlideNode(PathType.Linear, Node0Pos),
                new SlideNode(PathType.Linear, Node1Pos),
                new SlideNode(PathType.Linear, Node2Pos)
            };
        }

        // Covers DX L pattern 2-5
        public static SlideNode[] GenerateLPattern(int start, int end)
        {
            start = start.NormalizePath();

            end = Math.Clamp(end.NormalizePath(), start + 1, start + 4).NormalizePath();

            Vector2 Node0Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start);
            Vector2 Node1Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start + 6);
            Vector2 Node2Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, end);

            return new SlideNode[]{
                new SlideNode(PathType.Linear, Node0Pos),
                new SlideNode(PathType.Linear, Node1Pos),
                new SlideNode(PathType.Linear, Node2Pos)
            };
        }

        // DX Circle Pattern
        public static SlideNode[] GenerateCirclePattern(int start, int end, int rotation = +1)
        {
            start = start.NormalizePath();
            end = Math.Clamp(end.NormalizePath(), start + 1, start + 8);

            List<SlideNode> SlidePath = new List<SlideNode>();
            if (rotation >= 0)
                for (int i = start; i <= end; ++i)
                    SlidePath.Add(new SlideNode(PathType.PerfectCurve, SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, i)));
            else
                for (int i = end; i >= start; --i)
                    SlidePath.Add(new SlideNode(PathType.PerfectCurve, SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, i)));

            return SlidePath.ToArray();
        }

        public static SlideNode[] GenerateUPattern(int start, int end)
        {
            start = start.NormalizePath();
            end = end.NormalizePath();

            Vector2 Node0Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start);
            Vector2 Node1Pos = getPositionInBetween(Node0Pos, SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, start + 5));
            Vector2 Node2Pos = Vector2.Zero;
            Vector2 Node4Pos = SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, end);
            Vector2 Node3Pos = getPositionInBetween(Node4Pos, SentakkiExtensions.GetPathPosition(SentakkiPlayfield.INTERSECTDISTANCE, end + 3));

            return new SlideNode[]{
                new SlideNode(PathType.Linear, Node0Pos),
                new SlideNode(PathType.Linear, Node1Pos),
                new SlideNode(PathType.PerfectCurve, Node2Pos),
                new SlideNode(PathType.PerfectCurve, Node3Pos),
                new SlideNode(PathType.Linear, Node4Pos)
            };
        }
    }
}