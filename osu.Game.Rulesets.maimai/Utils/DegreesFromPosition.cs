using System;
using System.Collections.Generic;
using osuTK;

namespace osu.Game.Rulesets.Maimai
{
    public static class Utils
    {
        public static float GetDegreesFromPosition(this Vector2 target, Vector2 self)
        {
            Vector2 offset = self - target;
            float degrees = (float)MathHelper.RadiansToDegrees(Math.Atan2(-offset.X, offset.Y)) + 24.3f;

            return degrees;
        }
        public static float/*<int, Nullable<int>> */GetNotePathFromDegrees(float degrees)
        {
            degrees %= 360f;
            Console.WriteLine("Input: " + degrees.ToString());
            float SingleThreshold = 40f; // 40 Degrees margin from centre
            int result = 0;
            float[] pathAngles =
            {
                22.5f,
                67.5f,
                112.5f,
                157.5f,
                202.5f,
                247.5f,
                292.5f,
                337.5f
            };
            for (int i = 0; i < pathAngles.Length; ++i)
            {
                if (pathAngles[i] - degrees >= -22.5f && pathAngles[i] - degrees <= 22.5f)
                    result = i;

                //if (pathAngles[i] - degrees >= -45 && pathAngles[i] - degrees <= -SingleThreshold)
                //    return new Tuple<int, Nullable<int>>((i == 0 ? 7 : i), i);
                //else if (pathAngles[i] - degrees > -SingleThreshold && pathAngles[i] - degrees <= SingleThreshold)
                //    return new Tuple<int, Nullable<int>>(i, null);
                //else if (pathAngles[i] - degrees <= 45 && pathAngles[i] - degrees >= SingleThreshold)
                //    return new Tuple<int, Nullable<int>>(i, (i == 7 ? 0 : i));

            }
            Console.WriteLine("Output: " + pathAngles[result].ToString() + "(" + result.ToString() + ")");
            return pathAngles[result];
        }
    }
}
