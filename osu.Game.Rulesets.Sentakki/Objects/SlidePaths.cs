using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects;

public static class SlidePaths
{
    public static readonly List<(SlideSegment Segment, double MinDuration)> VALID_CONVERT_PATHS;

    static SlidePaths()
    {
        VALID_CONVERT_PATHS = [];

        for (PathShapes i = PathShapes.Straight; i <= PathShapes.Fan; ++i)
        {
            for (int j = 0; j < 8; ++j)
            {
                // Technically legal depending on the situation, but let's not allow converts to use them.
                if (i == PathShapes.Straight && j is <= 1 or >= 7)
                    continue;
                if (i == PathShapes.Circle && j is 7 or 1)
                    continue;

                for (int k = 0; k < 2; ++k)
                {
                    var tmp = new SlideSegment(i, j, k == 1);
                    if (CheckSlideValidity(tmp, true))
                        VALID_CONVERT_PATHS.Add((tmp, CreateSlidePath([tmp]).Sum(p => p.CalculatedDistance) / 10));
                }
            }
        }
    }

    // Checks if a slide is valid given parameters
    //
    // For converter use, we want to ensure that redundant mirrored shapes are discarded, to not skew the selection process.
    // We additionally block +-1 end offset straight/circle slides, as those do not play well when not done right.
    public static bool CheckSlideValidity(SlideSegment param, bool forConverterUse = false)
    {
        int normalizedEnd = param.RelativeEndLane.NormalizeLane();
        bool mirrored = param.Mirrored;

        switch (param.Shape)
        {
            // Straights always have redundant mirrors
            // Additionally, end offset being 1 or 7 can't be used by conversion
            case PathShapes.Straight:
                return normalizedEnd != 0 && (!forConverterUse || (!mirrored && normalizedEnd is not (1 or 7)));

            // Circular slide end offset being 1 or 7 can't be used by conversion
            case PathShapes.Circle:
                return !forConverterUse || normalizedEnd is not (1 or 7);

            case PathShapes.V:
                return (!mirrored || !forConverterUse) && normalizedEnd != 0;

            case PathShapes.U:
            case PathShapes.Cup:
                return true;

            case PathShapes.Fan:
                return (!mirrored || !forConverterUse) && normalizedEnd == 4;

            case PathShapes.Thunder:
                return normalizedEnd == 4;
        }

        return false;
    }

    public static IReadOnlyList<SliderPath> CreateSlidePath(IReadOnlyList<SlideSegment> pathParameters) => CreateSlidePath(0, pathParameters);

    public static IReadOnlyList<SliderPath> CreateSlidePath(int startOffset, IReadOnlyList<SlideSegment> pathParameters)
    {
        List<SliderPath> slideSegments = [];

        for (int i = 0; i < pathParameters.Count; ++i)
        {
            var path = pathParameters[i];

            switch (path.Shape)
            {
                case PathShapes.Straight:
                    slideSegments.Add(generateStraightPattern(startOffset, path.RelativeEndLane));
                    break;

                case PathShapes.Fan:
                    slideSegments.Add(generateStraightPattern(startOffset, 4));
                    break;

                case PathShapes.Circle:
                    slideSegments.Add(generateCirclePattern(startOffset, path.RelativeEndLane, path.Mirrored ? RotationDirection.Counterclockwise : RotationDirection.Clockwise));
                    break;

                case PathShapes.V:
                    slideSegments.Add(generateVPattern(startOffset, path.RelativeEndLane));
                    break;

                case PathShapes.U:
                    slideSegments.Add(generateUPattern(startOffset, path.RelativeEndLane, path.Mirrored));
                    break;

                case PathShapes.Cup:
                    slideSegments.Add(generateCupPattern(startOffset, path.RelativeEndLane, path.Mirrored));
                    break;

                case PathShapes.Thunder:
                    slideSegments.AddRange(generateThunderPattern(startOffset, path.Mirrored));
                    break;
            }

            startOffset += path.RelativeEndLane;
        }

        return slideSegments;
    }

    #region Generation methods

    private static Vector2 getPositionInBetween(Vector2 first, Vector2 second, float ratio = .5f) => first + (second - first) * ratio;

    // Covers DX Straight 3-7
    private static SliderPath generateStraightPattern(int startLane, int relativeEndLane)
    {
        return new SliderPath([
            new PathControlPoint(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, startLane), PathType.LINEAR),
            new PathControlPoint(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, relativeEndLane + startLane), PathType.LINEAR)
        ]);
    }

    private static Vector2 getIntesectPoint(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        float tmp = (b2.X - b1.X) * (a2.Y - a1.Y) - (b2.Y - b1.Y) * (a2.X - a1.X);
        float mu = ((a1.X - b1.X) * (a2.Y - a1.Y) - (a1.Y - b1.Y) * (a2.X - a1.X)) / tmp;

        return new Vector2(
            b1.X + (b2.X - b1.X) * mu,
            b1.Y + (b2.Y - b1.Y) * mu
        );
    }

    // Thunder pattern
    private static IEnumerable<SliderPath> generateThunderPattern(int startLane, bool mirrored = false)
    {
        int lane1 = (mirrored ? 3 : 5) + startLane;
        int lane2 = (mirrored ? 2 : 6) + startLane;
        int lane3 = (mirrored ? 6 : 2) + startLane;
        int lane4 = (mirrored ? 7 : 1) + startLane;

        Vector2 node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, startLane);
        Vector2 node1Pos = getIntesectPoint(lanestart(startLane), lanestart(lane1), lanestart(lane2), lanestart(lane3));

        Vector2 node2Pos = getIntesectPoint(lanestart(lane2), lanestart(lane3), lanestart(lane4), lanestart(4 + startLane));
        Vector2 node3Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 4 + startLane);

        return
        [
            new SliderPath([
                new PathControlPoint(node0Pos, PathType.LINEAR),
                new PathControlPoint(node1Pos, PathType.LINEAR),
                new PathControlPoint(node2Pos, PathType.LINEAR),
                new PathControlPoint(node3Pos, PathType.LINEAR)
            ])
        ];

        static Vector2 lanestart(int x) => SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, x);
    }

    // Covers DX V pattern 1-8
    private static SliderPath generateVPattern(int startLane, int relativeEndLane)
    {
        Vector2 node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, startLane);
        Vector2 node1Pos = Vector2.Zero;
        Vector2 node2Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, relativeEndLane + startLane);

        return new SliderPath([
            new PathControlPoint(node0Pos, PathType.LINEAR),
            new PathControlPoint(node1Pos, PathType.LINEAR),
            new PathControlPoint(node2Pos, PathType.LINEAR)
        ]);
    }

    // DX Circle Pattern
    private static SliderPath generateCirclePattern(int startLane, int relativeEndLane, RotationDirection direction = RotationDirection.Clockwise)
    {
        bool isFullCircle = relativeEndLane.NormalizeLane() == 0;
        bool isCounterClockwise = direction == RotationDirection.Counterclockwise;

        float startAngle = startLane.GetRotationForLane();
        float endAngle = (relativeEndLane + startLane).GetRotationForLane();

        float centreAngle;

        if (isFullCircle)
        {
            // If it is a full circle, we simply put the centre node across that start point
            centreAngle = (startLane + 4).GetRotationForLane();
        }
        else
        {
            // Find the angle between the start and end points
            centreAngle = (startAngle + endAngle) / 2;

            // If the direction is Counterclockwise, then we flip the centre to the otherside;
            if (isCounterClockwise)
                centreAngle += 180;
        }

        // This is a slight angle tweak to help SliderPath determine which direction the path goes
        float angleTweak = 0.1f * (isCounterClockwise ? -1 : 1);

        Vector2 node0Pos = MathExtensions.PointOnCircle(SentakkiPlayfield.INTERSECTDISTANCE, startAngle + angleTweak);
        Vector2 node1Pos = MathExtensions.PointOnCircle(SentakkiPlayfield.INTERSECTDISTANCE, centreAngle);
        Vector2 node2Pos = MathExtensions.PointOnCircle(SentakkiPlayfield.INTERSECTDISTANCE, endAngle);

        return new SliderPath([
            new PathControlPoint(node0Pos, PathType.PERFECT_CURVE),
            new PathControlPoint(node1Pos),
            new PathControlPoint(node2Pos, PathType.PERFECT_CURVE)
        ]);
    }

    private static SliderPath generateUPattern(int startLane, int relativeEndLane, bool reversed = false)
    {
        Vector2 node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, startLane);
        Vector2 node1Pos = getPositionInBetween(node0Pos, SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, (reversed ? 3 : 5) + startLane), .51f);

        float angleDiff = ((relativeEndLane + startLane).GetRotationForLane() + startLane.GetRotationForLane()) / 2 + (Math.Abs(relativeEndLane) > (reversed ? 3 : 4) ? 0 : 180);
        Vector2 node2Pos = MathExtensions.PointOnCircle(115, angleDiff);

        Vector2 node4Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, relativeEndLane + startLane);
        Vector2 node3Pos = getPositionInBetween(node4Pos, SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, relativeEndLane + (reversed ? -3 : 3) + startLane), .51f);

        return new SliderPath([
            new PathControlPoint(node0Pos, PathType.LINEAR),
            new PathControlPoint(node1Pos, PathType.PERFECT_CURVE),
            new PathControlPoint(node2Pos),
            new PathControlPoint(node3Pos, PathType.PERFECT_CURVE),
            new PathControlPoint(node4Pos, PathType.LINEAR)
        ]);
    }

    private static SliderPath generateCupPattern(int startLane, int relativeEndLane, bool mirrored = false)
    {
        const float r = 270 / 2f;

        int x = mirrored ? (-relativeEndLane).NormalizeLane() : relativeEndLane;

        float originAngle = 90;
        float angle1 = 300;
        float angle2 = 269;
        float angle3 = 230;

        float loopEndAngle = x switch
        {
            0 => 60,
            1 => 90,
            2 => 180,
            3 => 240,
            4 => 300,
            5 => 330,
            6 => 360,
            7 => 390,
            _ => 0
        };

        float offsetAdjustment = startLane.GetRotationForLane() - 22.5f;

        if (mirrored)
        {
            originAngle = -originAngle + 45;
            angle1 = -angle1 + 45;
            angle2 = -angle2 + 45;
            angle3 = -angle3 + 45;
            loopEndAngle = -loopEndAngle + 45;
        }

        Vector2 loopOrigin = MathExtensions.PointOnCircle(r, originAngle + offsetAdjustment);

        Vector2 node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, startLane);
        Vector2 node1Pos = loopOrigin + MathExtensions.PointOnCircle(r, angle1 + offsetAdjustment);
        Vector2 node2Pos = loopOrigin + MathExtensions.PointOnCircle(r, angle2 + offsetAdjustment);
        Vector2 node3Pos = loopOrigin + MathExtensions.PointOnCircle(r, angle3 + offsetAdjustment);
        Vector2 node4Pos = loopOrigin + MathExtensions.PointOnCircle(r, (angle3 + loopEndAngle) / 2 + (x >= 3 ? 180 : 0) + offsetAdjustment);
        Vector2 node5Pos = loopOrigin + MathExtensions.PointOnCircle(r, loopEndAngle + offsetAdjustment);
        Vector2 node6Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, relativeEndLane + startLane);

        return new SliderPath([
            new PathControlPoint(node0Pos, PathType.LINEAR),
            new PathControlPoint(node1Pos, PathType.PERFECT_CURVE),
            new PathControlPoint(node2Pos),
            new PathControlPoint(node3Pos, PathType.PERFECT_CURVE),
            new PathControlPoint(node4Pos),
            new PathControlPoint(node5Pos, PathType.PERFECT_CURVE),
            new PathControlPoint(node6Pos, PathType.LINEAR)
        ]);
    }

    #endregion
}
