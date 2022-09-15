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
        public static int FANID => VALIDPATHS.Count - 1;
        public static readonly List<(SentakkiSlidePath, SentakkiSlidePath)> VALIDPATHS = new List<(SentakkiSlidePath, SentakkiSlidePath)>{
            (GenerateCirclePattern(0, 2), GenerateCirclePattern(0, 6, RotationDirection.Counterclockwise)),
            (GenerateCirclePattern(0, 3), GenerateCirclePattern(0, 5, RotationDirection.Counterclockwise)),
            (GenerateCirclePattern(0, 4), GenerateCirclePattern(0, 4, RotationDirection.Counterclockwise)),
            (GenerateCirclePattern(0, 5), GenerateCirclePattern(0, 3, RotationDirection.Counterclockwise)),
            (GenerateCirclePattern(0, 6), GenerateCirclePattern(0, 2, RotationDirection.Counterclockwise)),
            (GenerateCirclePattern(0, 7), GenerateCirclePattern(0, 1, RotationDirection.Counterclockwise)),
            (GenerateCirclePattern(0, 8), GenerateCirclePattern(0, 0, RotationDirection.Counterclockwise)),
            (GenerateLPattern(0, 1), GenerateLPattern(0, 7, true)),
            (GenerateLPattern(0, 2), GenerateLPattern(0, 6, true)),
            (GenerateLPattern(0, 3), GenerateLPattern(0, 5, true)),
            (GenerateLPattern(0, 4), GenerateLPattern(0, 4, true)),
            (GenerateStraightPattern(0,2), GenerateStraightPattern(0,2)),
            (GenerateStraightPattern(0,3), GenerateStraightPattern(0,5)),
            (GenerateStraightPattern(0,4), null),
            (GenerateThunderPattern(0), GenerateThunderPattern(0,true)),
            (GenerateUPattern(0, 0), GenerateUPattern(0, 0, true)),
            (GenerateUPattern(0, 1), GenerateUPattern(0, 7, true)),
            (GenerateUPattern(0, 2), GenerateUPattern(0, 6, true)),
            (GenerateUPattern(0, 3), GenerateUPattern(0, 5, true)),
            (GenerateUPattern(0, 4), GenerateUPattern(0, 4, true)),
            (GenerateUPattern(0, 5), GenerateUPattern(0, 3, true)),
            (GenerateUPattern(0, 6), GenerateUPattern(0, 2, true)),
            (GenerateUPattern(0, 7), GenerateUPattern(0, 1, true)),
            (GenerateVPattern(0,1), GenerateVPattern(0,7)),
            (GenerateVPattern(0,2), GenerateVPattern(0,6)),
            (GenerateVPattern(0,3), GenerateVPattern(0,5)),
            (GenerateCupPattern(0, 0),GenerateCupPattern(0, 0, true)),
            (GenerateCupPattern(0, 1),GenerateCupPattern(0, 7, true)),
            (GenerateCupPattern(0, 2),GenerateCupPattern(0, 6, true)),
            (GenerateCupPattern(0, 3),GenerateCupPattern(0, 5, true)),
            (GenerateCupPattern(0, 4),GenerateCupPattern(0, 4, true)),
            (GenerateCupPattern(0, 5),GenerateCupPattern(0, 3, true)),
            (GenerateCupPattern(0, 6),GenerateCupPattern(0, 2, true)),
            (GenerateCupPattern(0, 7),GenerateCupPattern(0, 1, true)),

            (GenerateStraightPattern(0,4), null),//An extra entry for the Fan Slide
        };

        public static SentakkiSlidePath GetSlidePath(int ID, bool IsMirrored = false)
        {
            (var original, var mirrored) = VALIDPATHS[ID];
            if (IsMirrored)
                return mirrored ?? original;
            return original;
        }

        private static Vector2 getPositionInBetween(Vector2 first, Vector2 second, float ratio = .5f) => first + ((second - first) * ratio);

        // Covers DX Straight 3-7
        public static SentakkiSlidePath GenerateStraightPattern(int offset, int end)
        {
            var path = new SliderPath(new PathControlPoint[] {
                new PathControlPoint(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, offset), PathType.Linear),
                new PathControlPoint(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end), PathType.Linear),
            });

            return new SentakkiSlidePath(path, end + offset);
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
        public static SentakkiSlidePath GenerateThunderPattern(int offset, bool mirrored = false)
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

            SliderPath[] segments = new SliderPath[]{
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

            return new SentakkiSlidePath(segments, offset + 4);
        }

        // Covers DX V pattern 1-8
        public static SentakkiSlidePath GenerateVPattern(int offset, int end)
        {
            Vector2 Node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, offset);
            Vector2 Node1Pos = Vector2.Zero;
            Vector2 Node2Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + offset);



            if (end >= 3 && end <= 5)
            {
                var path = new SliderPath(new PathControlPoint[]{
                    new PathControlPoint(Node0Pos, PathType.Linear),
                    new PathControlPoint(Node1Pos, PathType.Linear),
                    new PathControlPoint(Node2Pos, PathType.Linear)
                });
                return new SentakkiSlidePath(path, end);
            }
            else
            {
                SliderPath[] segments = new SliderPath[]{
                    new SliderPath(new PathControlPoint[]{
                        new PathControlPoint(Node0Pos, PathType.Linear),
                        new PathControlPoint(Node1Pos, PathType.Linear),
                    }),
                    new SliderPath(new PathControlPoint[]{
                        new PathControlPoint(Node1Pos, PathType.Linear),
                        new PathControlPoint(Node2Pos, PathType.Linear),
                    })
                };

                return new SentakkiSlidePath(segments, end + offset);
            }
        }

        // Covers DX L pattern 2-5
        public static SentakkiSlidePath GenerateLPattern(int offset, int end, bool mirrored = false)
        {
            Vector2 Node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, offset);
            Vector2 Node1Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, (mirrored ? 2 : 6) + offset);
            Vector2 Node2Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + offset);

            var segments = new SliderPath[]{
                new SliderPath(new PathControlPoint[]{
                    new PathControlPoint(Node0Pos, PathType.Linear),
                    new PathControlPoint(Node1Pos, PathType.Linear),
                }),
                new SliderPath(new PathControlPoint[]{
                    new PathControlPoint(Node1Pos, PathType.Linear),
                    new PathControlPoint(Node2Pos, PathType.Linear),
                }),
            };

            return new SentakkiSlidePath(segments, end + offset);
        }

        // DX Circle Pattern
        public static SentakkiSlidePath GenerateCirclePattern(int offset, int end, RotationDirection direction = RotationDirection.Clockwise)
        {
            float centre = ((offset.GetRotationForLane() + (end + offset).GetRotationForLane()) / 2) + (direction == RotationDirection.Counterclockwise ? 180 : 0);
            Vector2 centreNode = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, centre == offset.GetRotationForLane() ? centre + 180 : centre);

            var path = new SliderPath(new PathControlPoint[]{
                new PathControlPoint(SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, offset.GetRotationForLane() + (direction == RotationDirection.Counterclockwise ? -.5f : .5f)), PathType.PerfectCurve),
                new PathControlPoint(centreNode),
                new PathControlPoint(SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end+offset), PathType.PerfectCurve)
            });

            return new SentakkiSlidePath(path, end + offset);
        }

        public static SentakkiSlidePath GenerateUPattern(int offset, int end, bool reversed = false)
        {
            Vector2 Node0Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, offset);
            Vector2 Node1Pos = getPositionInBetween(Node0Pos, SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, (reversed ? 3 : 5) + offset), .51f);

            float angleDiff = (((end + offset).GetRotationForLane() + offset.GetRotationForLane()) / 2) + (Math.Abs(end) > (reversed ? 3 : 4) ? 0 : 180);
            Vector2 Node2Pos = SentakkiExtensions.GetCircularPosition(115, angleDiff);

            Vector2 Node4Pos = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + offset);
            Vector2 Node3Pos = getPositionInBetween(Node4Pos, SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, end + (reversed ? -3 : 3) + offset), .51f);

            var path = new SliderPath(new PathControlPoint[]{
                new PathControlPoint(Node0Pos,PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.PerfectCurve),
                new PathControlPoint(Node2Pos),
                new PathControlPoint(Node3Pos, PathType.PerfectCurve),
                new PathControlPoint(Node4Pos,PathType.Linear)
            });
            return new SentakkiSlidePath(path, end + offset);
        }

        public static SentakkiSlidePath GenerateCupPattern(int offset, int end, bool mirrored = false)
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

            var path = new SliderPath(new PathControlPoint[]{
                new PathControlPoint(Node0Pos, PathType.Linear),
                new PathControlPoint(Node1Pos, PathType.PerfectCurve),
                new PathControlPoint(Node2Pos),
                new PathControlPoint(Node3Pos, PathType.PerfectCurve),
                new PathControlPoint(Node4Pos),
                new PathControlPoint(Node5Pos,PathType.PerfectCurve),
                new PathControlPoint(Node6Pos, PathType.Linear)
            });

            return new SentakkiSlidePath(path, end);
        }
    }
}
