using System;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Extensions;

public static class MathExtensions
{
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
    public static float AngleDelta(float a, float b) => (b - a).Mod(360);

    /// <summary>
    /// Computes the angle (in degrees) of <c>target</c> around <c>origin</c>
    /// </summary>
    public static float AngleTo(this Vector2 origin, Vector2 target)
    {
        Vector2 direction = target - origin;

        float angle = MathHelper.RadiansToDegrees(MathF.Atan2(direction.Y, direction.X));

        return (angle + 90).NormalizeAngle();
    }

    /// <summary>
    /// Computes a vector with the same angle as <c>angle</c>, and is along the circumference with radius <c>distance</c>
    /// </summary>
    /// <param name="circleRadius">Length of the vector</param>
    /// <param name="angle">Angle of vector in degrees</param>
    public static Vector2 PointOnCircle(float circleRadius, float angle)
    {
        // This is offset by 90 degrees since I use the vertical-up system (like a compass)
        float radians = MathHelper.DegreesToRadians(angle + 90);

        (float sin, float cos) = MathF.SinCos(radians);

        // Taking advantage of the assumption is that the y axis points downwards
        // Only I use this, so it's acceptable
        return new Vector2(-circleRadius * cos, -circleRadius * sin);
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
}
