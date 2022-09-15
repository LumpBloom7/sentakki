using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
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

        public static readonly List<(PathParameters parameters, double MinDuration)> VALIDPATHS;
        static SlidePaths()
        {
            VALIDPATHS = new List<(PathParameters, double)>();
            for (PathShapes i = PathShapes.Straight; i <= PathShapes.Fan; ++i)
                for (int j = 0; j < 8; ++j)
                    for (int k = 0; k < 2; ++k)
                    {
                        var tmp = new PathParameters(i, j, k == 1);
                        if (CheckSlideValidity(tmp))
                            VALIDPATHS.Add((tmp, CreateSlidePath(tmp).MinDuration));
                    }
        }

        // Checks if a slide is valid given parameters
        public static bool CheckSlideValidity(PathParameters param)
        {
            int normalizedEnd = param.EndOffset;
            bool mirrored = param.Mirrored;

            switch (param.Shape)
            {
                case PathShapes.Straight:
                    return normalizedEnd > 1 && normalizedEnd < 7;

                case PathShapes.Circle:
                    return mirrored ? normalizedEnd != 0 : normalizedEnd != 7;

                case PathShapes.V:
                    return normalizedEnd != 4;

                case PathShapes.L:
                    return mirrored ? normalizedEnd < 5 : normalizedEnd > 3;

                case PathShapes.U:
                case PathShapes.Cup:
                case PathShapes.Thunder:
                    return true;

                case PathShapes.Fan:
                    return normalizedEnd == 4;
            }

            return false;
        }

        public static SentakkiSlidePath CreateSlidePath(params PathParameters[] pathParameters) => CreateSlidePath(0, pathParameters);
        public static SentakkiSlidePath CreateSlidePath(int startOffset, params PathParameters[] pathParameters)
        {
            List<SliderPath> slideSegments = new List<SliderPath>();

            foreach (var path in pathParameters)
            {
                switch (path.Shape)
                {
                    case PathShapes.Straight:
                    case PathShapes.Fan:
                        slideSegments.Add(generateStraightPattern(startOffset, path.EndOffset));
                        break;

                    case PathShapes.Circle:
                        slideSegments.Add(generateCirclePattern(startOffset, path.EndOffset, path.Mirrored ? RotationDirection.Counterclockwise : RotationDirection.Clockwise));
                        break;

                    case PathShapes.V:
                        slideSegments.AddRange(generateVPattern(startOffset, path.EndOffset));
                        break;

                    case PathShapes.L:
                        slideSegments.AddRange(generateLPattern(startOffset, path.EndOffset));
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

            return new SentakkiSlidePath(slideSegments.ToArray(), startOffset);
        }

        #region Generation methods

        private static Vector2 getPositionInBetween(Vector2 first, Vector2 second, float ratio = .5f) => first + ((second - first) * ratio);

        // Covers DX Straight 3-7
        private static SliderPath generateStraightPattern(int offset, int end)
        {
            return new SliderPath(new PathControlPoint[] {
                new PathControlPoint(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, offset), PathType.Linear),
                new PathControlPoint(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end), PathType.Linear),
            });
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
        private static IEnumerable<SliderPath> generateThunderPattern(int offset, bool mirrored = false)
        {
            int lane1 = (mirrored ? 3 : 5) + offset;
            int lane2 = (mirrored ? 2 : 6) + offset;
            int lane3 = (mirrored ? 6 : 2) + offset;
            int lane4 = (mirrored ? 7 : 1) + offset;

            static Vector2 lanestart(int x) => SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, x);
            Vector2 Node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, offset);
            Vector2 Node1Pos = getIntesectPoint(lanestart(offset), lanestart(lane1), lanestart(lane2), lanestart(lane3));

            Vector2 Node2Pos = getIntesectPoint(lanestart(lane2), lanestart(lane3), lanestart(lane4), lanestart(4 + offset));
            Vector2 Node3Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 4 + offset);

            return new SliderPath[]{
                new SliderPath(new PathControlPoint[]{
                    new PathControlPoint(Node0Pos, PathType.Linear),
                    new PathControlPoint(Node1Pos, PathType.Linear),
                }),
                new SliderPath(new PathControlPoint[]{
                    new PathControlPoint(Node1Pos, PathType.Linear),
                    new PathControlPoint(Node2Pos, PathType.Linear),
                }),
                new SliderPath(new PathControlPoint[]{
                    new PathControlPoint(Node2Pos, PathType.Linear),
                    new PathControlPoint(Node3Pos, PathType.Linear),
                })
            };
        }

        // Covers DX V pattern 1-8
        private static IEnumerable<SliderPath> generateVPattern(int offset, int end)
        {
            Vector2 Node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, offset);
            Vector2 Node1Pos = Vector2.Zero;
            Vector2 Node2Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + offset);

            if (end >= 3 && end <= 5)
            {
                yield return new SliderPath(new PathControlPoint[]{
                    new PathControlPoint(Node0Pos, PathType.Linear),
                    new PathControlPoint(Node1Pos, PathType.Linear),
                    new PathControlPoint(Node2Pos, PathType.Linear)
                });
            }
            else
            {
                yield return new SliderPath(new PathControlPoint[]{
                    new PathControlPoint(Node0Pos, PathType.Linear),
                    new PathControlPoint(Node1Pos, PathType.Linear),
                });

                yield return new SliderPath(new PathControlPoint[]{
                    new PathControlPoint(Node1Pos, PathType.Linear),
                    new PathControlPoint(Node2Pos, PathType.Linear),
                });
            };
        }

        // Covers DX L pattern 2-5
        private static IEnumerable<SliderPath> generateLPattern(int offset, int end, bool mirrored = false)
        {
            Vector2 Node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, offset);
            Vector2 Node1Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, (mirrored ? 2 : 6) + offset);
            Vector2 Node2Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + offset);

            yield return new SliderPath(new PathControlPoint[]{
                new PathControlPoint(Node0Pos, PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.Linear),
            });

            yield return new SliderPath(new PathControlPoint[]{
                new PathControlPoint(Node1Pos, PathType.Linear),
                new PathControlPoint(Node2Pos, PathType.Linear),
            });
        }

        // DX Circle Pattern
        private static SliderPath generateCirclePattern(int offset, int end, RotationDirection direction = RotationDirection.Clockwise)
        {
            float centre = ((offset.GetRotationForLane() + (end + offset).GetRotationForLane()) / 2) + (direction == RotationDirection.Counterclockwise ? 180 : 0);
            Vector2 centreNode = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, centre == offset.GetRotationForLane() ? centre + 180 : centre);

            return new SliderPath(new PathControlPoint[]{
                new PathControlPoint(SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, offset.GetRotationForLane() + (direction == RotationDirection.Counterclockwise ? -.5f : .5f)), PathType.PerfectCurve),
                new PathControlPoint(centreNode),
                new PathControlPoint(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end+offset), PathType.PerfectCurve)
            });
        }

        private static SliderPath generateUPattern(int offset, int end, bool reversed = false)
        {
            Vector2 Node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, offset);
            Vector2 Node1Pos = getPositionInBetween(Node0Pos, SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, (reversed ? 3 : 5) + offset), .51f);

            float angleDiff = (((end + offset).GetRotationForLane() + offset.GetRotationForLane()) / 2) + (Math.Abs(end) > (reversed ? 3 : 4) ? 0 : 180);
            Vector2 Node2Pos = SentakkiExtensions.GetCircularPosition(115, angleDiff);

            Vector2 Node4Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + offset);
            Vector2 Node3Pos = getPositionInBetween(Node4Pos, SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + (reversed ? -3 : 3) + offset), .51f);

            return new SliderPath(new PathControlPoint[]{
                new PathControlPoint(Node0Pos,PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.PerfectCurve),
                new PathControlPoint(Node2Pos),
                new PathControlPoint(Node3Pos, PathType.PerfectCurve),
                new PathControlPoint(Node4Pos,PathType.Linear)
            });
        }

        private static SliderPath generateCupPattern(int offset, int end, bool mirrored = false)
        {
            float r = 270 / 2f;

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

            Vector2 Node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, offset);
            Vector2 Node1Pos = loopOrigin + SentakkiExtensions.GetCircularPosition(r, angle1 + offsetAdjustment);
            Vector2 Node2Pos = loopOrigin + SentakkiExtensions.GetCircularPosition(r, angle2 + offsetAdjustment);
            Vector2 Node3Pos = loopOrigin + SentakkiExtensions.GetCircularPosition(r, angle3 + offsetAdjustment);
            Vector2 Node4Pos = loopOrigin + SentakkiExtensions.GetCircularPosition(r, ((angle3 + loopEndAngle) / 2) + (x >= 3 ? 180 : 0) + offsetAdjustment);
            Vector2 Node5Pos = loopOrigin + SentakkiExtensions.GetCircularPosition(r, loopEndAngle + offsetAdjustment);
            Vector2 Node6Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + offset);

            return new SliderPath(new PathControlPoint[]{
                new PathControlPoint(Node0Pos, PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.PerfectCurve),
                new PathControlPoint(Node2Pos),
                new PathControlPoint(Node3Pos, PathType.PerfectCurve),
                new PathControlPoint(Node4Pos),
                new PathControlPoint(Node5Pos,PathType.PerfectCurve),
                new PathControlPoint(Node6Pos, PathType.Linear)
            });
        }
        #endregion
    }
}
