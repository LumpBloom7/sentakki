using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Extensions;

public static class HitObjectExtensions
{
    public static Vector2 GetPosition(this HitObject hitObject)
        => ((IHasPosition)hitObject).Position;

    public static Vector2 GetEndPosition(this HitObject hitObject)
    {
        var startPos = hitObject.GetPosition();

        if (hitObject is IHasPathWithRepeats slider)
            return startPos + slider.Path.PositionAt(1);

        return startPos;
    }
}
