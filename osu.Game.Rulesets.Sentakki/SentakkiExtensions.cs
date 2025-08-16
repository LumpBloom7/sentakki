using System;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki;

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
    public static float GetDeltaAngle(float a, float b) => (b - a).Mod(360);

    /// <summary>
    /// Gives the playfield rotation of the lane matching the corresponding laneNumber
    /// </summary>
    public static float GetRotationForLane(this int laneNumber) => 22.5f + laneNumber * 45;

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

    /// <summary>
    /// A mathematically correct modulo operator. Not to be confused with the % operator which is a remainder operator.
    /// </summary>
    /// <param name="a">The dividend</param>
    /// <param name="b">The divisor</param>
    /// <returns>The modulus resulting from dividing a by b.</returns>
    public static int Mod(this int a, int b)
        => (a %= b) < 0 ? a + b : a;

    /// <summary>
    /// A mathematically correct modulo operator. Not to be confused with the % operator which is a remainder operator.
    /// </summary>
    /// <param name="a">The dividend</param>
    /// <param name="b">The divisor</param>
    /// <returns>The modulus resulting from dividing a by b.</returns>
    public static float Mod(this float a, float b)
        => (a %= b) < 0 ? a + b : a;

    public static ColourInfo ForSentakkiResult(this OsuColour osuColour, HitResult result)
    {
        switch (result)
        {
            case HitResult.Perfect:
                return ColourInfo.GradientVertical(Color4Extensions.FromHex("#7CF6FF"), Color4Extensions.FromHex("#FF9AD7"));

            default:
                return osuColour.ForHitResult(result);
        }
    }

    public static string GetDisplayNameForSentakkiResult(this HitResult result)
    {
        switch (result)
        {
            case HitResult.Perfect:
                return "Critical";

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

    public static Color4 LightenHsl(this Color4 colour, float ratio)
    {
        float r = colour.R + (1 - colour.R) * ratio;
        float g = colour.G + (1 - colour.G) * ratio;
        float b = colour.B + (1 - colour.B) * ratio;

        return new Color4(r, g, b, colour.A);
    }
}
