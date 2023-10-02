using System;
using osu.Framework.Extensions;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki
{
    public static class SentakkiExtensions
    {
        /// <summary>
        /// Normalizes the lane number to be between [0,8)
        /// </summary>
        public static int NormalizePath(this int laneNumber)
            => laneNumber.Mod(8);

        /// <summary>
        /// Normalizes the angle to be between [0,360)
        /// </summary>
        public static float NormalizeAngle(this float angle)
            => angle.Mod(360);

        /// <summary>
        /// Gets the minimum absolute angle (in degrees) difference between <c>a</c> and <c>b</c>
        /// </summary>
        /// <example>
        /// <code>
        /// a = 50
        /// b = 30
        /// GetDeltaAngle(a,b) = -20
        /// </code>
        /// </example>
        public static float GetDeltaAngle(float a, float b)
        {
            float delta = b - a;

            delta.Mod(360);

            if (delta < -180)
                delta += 360;
            else if (delta > 180)
                delta -= 360;

            return delta;
        }

        /// <summary>
        /// Gives the playfield rotation of the lane matching the corresponding laneNumber
        /// </summary>
        public static float GetRotationForLane(this int laneNumber) => 22.5f + (laneNumber * 45);

        public static Vector2 GetPositionAlongLane(float distance, int lane) => GetCircularPosition(distance, lane.GetRotationForLane());

        /// <summary>
        /// Computes a vector with the same angle as <c>angle</c>, and is along the circumference with radius <c>distance</c>
        /// </summary>
        /// <param name="distance">Length of the vector</param>
        /// <param name="angle">Angle of vector in degrees</param>
        public static Vector2 GetCircularPosition(float distance, float angle)
        {
            // This is offset by 90 degrees since I use the vertical-up system (like a compass)
            float radians = MathHelper.DegreesToRadians(angle + 90);

            (float sin, float cos) = MathF.SinCos(radians);

            // Taking advantage of the assumption is that the y axis points downwards
            // Only I use this, so it's acceptable
            return new Vector2(-distance * cos, -distance * sin);
        }

        /// <summary>
        /// Computes the angle (in degrees) of <c>target</c> around <c>origin</c>
        /// </summary>
        public static float GetDegreesFromPosition(this Vector2 origin, Vector2 target)
        {
            Vector2 direction = target - origin;

            float angle = MathHelper.RadiansToDegrees(MathF.Atan2(direction.Y, direction.X));

            return (angle + 90).NormalizeAngle();
        }

        public static int Mod(this int a, int b)
            => (a %= b) < 0 ? a + b : a;

        public static float Mod(this float a, float b)
            => (a %= b) < 0 ? a + b : a;

        public static Color4 GetColorForSentakkiResult(this HitResult result)
        {
            switch (result)
            {
                case HitResult.Great:
                    return Color4.Orange;

                case HitResult.Good:
                    return Color4.DeepPink;

                case HitResult.Ok:
                    return Color4.Green;

                default:
                    return Color4.LightGray;
            }
        }

        public static string GetDisplayNameForSentakkiResult(this HitResult result)
        {
            switch (result)
            {
                case HitResult.LargeBonus:
                    return "Critical Break Bonus";

                case HitResult.Great:
                    return "Perfect";

                case HitResult.Good:
                    return "Great";

                case HitResult.Ok:
                    return "Good";

                default:
                    return result.GetDescription();
            }
        }

        public static int GetNoteLaneFromDegrees(this float degrees)
        {
            if (degrees < 0) degrees += 360;
            if (degrees >= 360) degrees %= 360;

            int lane = (int)MathF.Round((degrees - 22.5f) / 45f);
            if (lane >= 8) lane -= 8;

            return lane;
        }
    }
}
