using System;
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
    }
}
