using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public static class SlidePaths
    {
        public enum PathShapes
        {
            Straight,
            Circle,
            V,
            L,
            U,
            Cup,
            Thunder,
            Fan,
        }

        public static readonly List<(SlideBodyPart parameters, double MinDuration)> VALIDPATHS;

        static SlidePaths()
        {
            VALIDPATHS = new List<(SlideBodyPart, double)>();

            for (PathShapes i = PathShapes.Straight; i <= PathShapes.Fan; ++i)
            {
                for (int j = 0; j < 8; ++j)
                {
                    for (int k = 0; k < 2; ++k)
                    {
                        var tmp = new SlideBodyPart(i, j, k == 1);
                        if (CheckSlideValidity(tmp, true))
                            VALIDPATHS.Add((tmp, CreateSlidePath(tmp).MinDuration));
                    }
                }
            }
        }

        // Checks if a slide is valid given parameters
        //
        // Discarding redundant mirrors should be used making a list of all the shapes, as to not get identical shapes
        // Not discarding them allows leniency in the check, so that a identical path can still be placed, without needing the mapper to explicitly turn off mirroring for a part.
        public static bool CheckSlideValidity(SlideBodyPart param, bool discardRedundantMirrors = false)
        {
            int normalizedEnd = param.EndOffset.NormalizePath();
            bool mirrored = param.Mirrored;

            switch (param.Shape)
            {
                case PathShapes.Straight:
                    return (!mirrored || !discardRedundantMirrors) && normalizedEnd > 1 && normalizedEnd < 7;

                case PathShapes.Circle:
                    return mirrored ? normalizedEnd != 7 : normalizedEnd != 1;

                case PathShapes.V:
                    return (!mirrored || !discardRedundantMirrors) && normalizedEnd != 0;

                case PathShapes.L:
                    return normalizedEnd != 0 && (mirrored ? normalizedEnd > 3 : normalizedEnd < 5);

                case PathShapes.U:
                case PathShapes.Cup:
                    return true;

                case PathShapes.Fan:
                    return (!mirrored || !discardRedundantMirrors) && normalizedEnd == 4;

                case PathShapes.Thunder:
                    return normalizedEnd == 4;
            }

            return false;
        }

        public static SentakkiSlidePath CreateSlidePath(params SlideBodyPart[] pathParameters) => CreateSlidePath(0, pathParameters);

        public static SentakkiSlidePath CreateSlidePath(int startOffset, params SlideBodyPart[] pathParameters)
        {
            List<SliderPath> slideSegments = new List<SliderPath>();

            for (int i = 0; i < pathParameters.Length; ++i)
            {
                var path = pathParameters[i];

                switch (path.Shape)
                {
                    case PathShapes.Straight:
                        slideSegments.Add(generateStraightPattern(startOffset, path.EndOffset));
                        break;

                    case PathShapes.Fan:
                        slideSegments.Add(generateStraightPattern(startOffset, 4));
                        break;

                    case PathShapes.Circle:

                        var newSegment = generateCirclePattern(startOffset, path.EndOffset, path.Mirrored ? RotationDirection.Counterclockwise : RotationDirection.Clockwise);

                        // Combine Circle paths in the same direction
                        if (i > 0)
                        {
                            var prevPath = pathParameters[i - 1];

                            if (prevPath.Shape == PathShapes.Circle && prevPath.Mirrored == path.Mirrored)
                            {
                                slideSegments[^1].ControlPoints.AddRange(newSegment.ControlPoints);
                                break;
                            }
                        }

                        slideSegments.Add(newSegment);
                        break;

                    case PathShapes.V:
                        slideSegments.AddRange(generateVPattern(startOffset, path.EndOffset));
                        break;

                    case PathShapes.L:
                        slideSegments.AddRange(generateLPattern(startOffset, path.EndOffset, path.Mirrored));
                        break;

                    case PathShapes.U:
                        slideSegments.Add(generateUPattern(startOffset, path.EndOffset, path.Mirrored));
                        break;

                    case PathShapes.Cup:
                        slideSegments.Add(generateCupPattern(startOffset, path.EndOffset, path.Mirrored));
                        break;

                    case PathShapes.Thunder:
                        slideSegments.AddRange(generateThunderPattern(startOffset, path.Mirrored));
                        break;
                }

                startOffset += path.EndOffset;

            }

            return new SentakkiSlidePath(slideSegments.ToArray(), startOffset, pathParameters[^1].Shape == PathShapes.Fan);
        }

        #region Generation methods

        private static Vector2 getPositionInBetween(Vector2 first, Vector2 second, float ratio = .5f) => first + ((second - first) * ratio);

        // Covers DX Straight 3-7
        private static SliderPath generateStraightPattern(int offset, int end)
        {
            return new SliderPath(new[]
            {
                new PathControlPoint(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, offset), PathType.Linear),
                new PathControlPoint(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + offset), PathType.Linear),
            });
        }

        private static Vector2 getIntesectPoint(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            float tmp = ((b2.X - b1.X) * (a2.Y - a1.Y)) - ((b2.Y - b1.Y) * (a2.X - a1.X));
            float mu = (((a1.X - b1.X) * (a2.Y - a1.Y)) - ((a1.Y - b1.Y) * (a2.X - a1.X))) / tmp;

            return new Vector2(
                b1.X + ((b2.X - b1.X) * mu),
                b1.Y + ((b2.Y - b1.Y) * mu)
            );
        }

        // Thunder pattern
        private static IEnumerable<SliderPath> generateThunderPattern(int offset, bool mirrored = false)
        {
            int lane1 = (mirrored ? 3 : 5) + offset;
            int lane2 = (mirrored ? 2 : 6) + offset;
            int lane3 = (mirrored ? 6 : 2) + offset;
            int lane4 = (mirrored ? 7 : 1) + offset;

            static Vector2 lanestart(int x) => SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, x);
            Vector2 node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, offset);
            Vector2 node1Pos = getIntesectPoint(lanestart(offset), lanestart(lane1), lanestart(lane2), lanestart(lane3));

            Vector2 node2Pos = getIntesectPoint(lanestart(lane2), lanestart(lane3), lanestart(lane4), lanestart(4 + offset));
            Vector2 node3Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 4 + offset);

            return new[]
            {
                new SliderPath(new[]
                {
                    new PathControlPoint(node0Pos, PathType.Linear),
                    new PathControlPoint(node1Pos, PathType.Linear),
                }),
                new SliderPath(new[]
                {
                    new PathControlPoint(node1Pos, PathType.Linear),
                    new PathControlPoint(node2Pos, PathType.Linear),
                }),
                new SliderPath(new[]
                {
                    new PathControlPoint(node2Pos, PathType.Linear),
                    new PathControlPoint(node3Pos, PathType.Linear),
                })
            };
        }

        // Covers DX V pattern 1-8
        private static IEnumerable<SliderPath> generateVPattern(int offset, int end)
        {
            Vector2 node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, offset);
            Vector2 node1Pos = Vector2.Zero;
            Vector2 node2Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + offset);

            if (end >= 3 && end <= 5)
            {
                yield return new SliderPath(new[]
                {
                    new PathControlPoint(node0Pos, PathType.Linear),
                    new PathControlPoint(node1Pos, PathType.Linear),
                    new PathControlPoint(node2Pos, PathType.Linear)
                });
            }
            else
            {
                yield return new SliderPath(new[]
                {
                    new PathControlPoint(node0Pos, PathType.Linear),
                    new PathControlPoint(node1Pos, PathType.Linear),
                });

                yield return new SliderPath(new[]
                {
                    new PathControlPoint(node1Pos, PathType.Linear),
                    new PathControlPoint(node2Pos, PathType.Linear),
                });
            }
        }

        // Covers DX L pattern 2-5
        private static IEnumerable<SliderPath> generateLPattern(int offset, int end, bool mirrored = false)
        {
            Vector2 node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, offset);
            Vector2 node1Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, (mirrored ? 2 : 6) + offset);
            Vector2 node2Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + offset);

            yield return new SliderPath(new[]
            {
                new PathControlPoint(node0Pos, PathType.Linear),
                new PathControlPoint(node1Pos, PathType.Linear),
            });

            yield return new SliderPath(new[]
            {
                new PathControlPoint(node1Pos, PathType.Linear),
                new PathControlPoint(node2Pos, PathType.Linear),
            });
        }

        // DX Circle Pattern
        private static SliderPath generateCirclePattern(int offset, int end, RotationDirection direction = RotationDirection.Clockwise)
        {
            bool isFullCircle = end.NormalizePath() == 0;
            bool isCounterClockwise = direction == RotationDirection.Counterclockwise;

            float startAngle = offset.GetRotationForLane();
            float endAngle = (end + offset).GetRotationForLane();

            float centreAngle;

            if (isFullCircle)
            {
                // If it is a full circle, we simply put the centre node across that start point
                centreAngle = (offset + 4).GetRotationForLane();
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

            Vector2 node0Pos = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, startAngle + angleTweak);
            Vector2 node1Pos = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, centreAngle);
            Vector2 node2Pos = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, endAngle);

            return new SliderPath(new[]
            {
                new PathControlPoint(node0Pos, PathType.PerfectCurve),
                new PathControlPoint(node1Pos),
                new PathControlPoint(node2Pos, PathType.PerfectCurve)
            });
        }

        private static SliderPath generateUPattern(int offset, int end, bool reversed = false)
        {
            Vector2 node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, offset);
            Vector2 node1Pos = getPositionInBetween(node0Pos, SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, (reversed ? 3 : 5) + offset), .51f);

            float angleDiff = (((end + offset).GetRotationForLane() + offset.GetRotationForLane()) / 2) + (Math.Abs(end) > (reversed ? 3 : 4) ? 0 : 180);
            Vector2 node2Pos = SentakkiExtensions.GetCircularPosition(115, angleDiff);

            Vector2 node4Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + offset);
            Vector2 node3Pos = getPositionInBetween(node4Pos, SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + (reversed ? -3 : 3) + offset), .51f);

            return new SliderPath(new[]
            {
                new PathControlPoint(node0Pos, PathType.Linear),
                new PathControlPoint(node1Pos, PathType.PerfectCurve),
                new PathControlPoint(node2Pos),
                new PathControlPoint(node3Pos, PathType.PerfectCurve),
                new PathControlPoint(node4Pos, PathType.Linear)
            });
        }

        private static SliderPath generateCupPattern(int offset, int end, bool mirrored = false)
        {
            const float r = 270 / 2f;

            int x = mirrored ? (-end).NormalizePath() : end;

            float originAngle = 90;
            float angle1 = 300;
            float angle2 = 269;
            float angle3 = 230;

            float loopEndAngle = 0;

            if (x == 0) loopEndAngle = 60;
            else if (x == 1) loopEndAngle = 90;
            else if (x == 2) loopEndAngle = 180;
            else if (x == 3) loopEndAngle = 240;
            else if (x == 4) loopEndAngle = 300;
            else if (x == 5) loopEndAngle = 330;
            else if (x == 6) loopEndAngle = 360;
            else if (x == 7) loopEndAngle = 390;

            float offsetAdjustment = offset.GetRotationForLane() - 22.5f;

            if (mirrored)
            {
                originAngle = -originAngle + 45;
                angle1 = -angle1 + 45;
                angle2 = -angle2 + 45;
                angle3 = -angle3 + 45;
                loopEndAngle = -loopEndAngle + 45;
            }

            Vector2 loopOrigin = SentakkiExtensions.GetCircularPosition(r, originAngle + offsetAdjustment);

            Vector2 node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, offset);
            Vector2 node1Pos = loopOrigin + SentakkiExtensions.GetCircularPosition(r, angle1 + offsetAdjustment);
            Vector2 node2Pos = loopOrigin + SentakkiExtensions.GetCircularPosition(r, angle2 + offsetAdjustment);
            Vector2 node3Pos = loopOrigin + SentakkiExtensions.GetCircularPosition(r, angle3 + offsetAdjustment);
            Vector2 node4Pos = loopOrigin + SentakkiExtensions.GetCircularPosition(r, ((angle3 + loopEndAngle) / 2) + (x >= 3 ? 180 : 0) + offsetAdjustment);
            Vector2 node5Pos = loopOrigin + SentakkiExtensions.GetCircularPosition(r, loopEndAngle + offsetAdjustment);
            Vector2 node6Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + offset);

            return new SliderPath(new[]
            {
                new PathControlPoint(node0Pos, PathType.Linear),
                new PathControlPoint(node1Pos, PathType.PerfectCurve),
                new PathControlPoint(node2Pos),
                new PathControlPoint(node3Pos, PathType.PerfectCurve),
                new PathControlPoint(node4Pos),
                new PathControlPoint(node5Pos, PathType.PerfectCurve),
                new PathControlPoint(node6Pos, PathType.Linear)
            });
        }

        #endregion
    }
}
