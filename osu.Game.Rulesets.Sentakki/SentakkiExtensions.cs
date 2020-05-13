
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using System;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Sentakki
{
    public static class SentakkiExtensions
    {
        public static Vector2 GetPosition(float distance, float angle)
        {
            return new Vector2(-(distance * (float)Math.Cos((angle + 90f) * (float)(Math.PI / 180))), -(distance * (float)Math.Sin((angle + 90f) * (float)(Math.PI / 180))));
        }

        public static float GetDegreesFromPosition(this Vector2 target, Vector2 self)
        {
            return (float)MathHelper.RadiansToDegrees(Math.Atan2(target.X - self.X, target.Y - self.Y));
        }

        public static float GetNotePathFromDegrees(this float degrees)
        {
            if (degrees < 0) degrees += 360;
            if (degrees >= 360) degrees %= 360;
            int result = 0;

            for (int i = 0; i < SentakkiPlayfield.PATHANGLES.Length; ++i)
            {
                if (SentakkiPlayfield.PATHANGLES[i] - degrees >= -22.5f && SentakkiPlayfield.PATHANGLES[i] - degrees <= 22.5f)
                    result = i;
            }
            return SentakkiPlayfield.PATHANGLES[result];
        }
    }
}
