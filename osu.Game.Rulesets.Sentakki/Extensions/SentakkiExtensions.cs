using System;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Extensions;

public static class SentakkiExtensions
{
    /// <summary>
    /// Normalizes the lane number to be between [0,8)
    /// </summary>
    public static int NormalizeLane(this int laneNumber)
        => laneNumber.Mod(8);

    public static int GetNoteLaneFromDegrees(this float degrees)
    {
        degrees = degrees.Mod(360);

        return (int)MathF.Round((degrees - 22.5f) / 45f);
    }

    /// <summary>
    /// Gives the playfield rotation of the lane matching the corresponding laneNumber
    /// </summary>
    public static float GetRotationForLane(this int laneNumber) => 22.5f + laneNumber * 45;

    public static Vector2 GetPositionAlongLane(float distance, int lane) => MathExtensions.PointOnCircle(distance, lane.GetRotationForLane());

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

            case HitResult.Meh:
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
