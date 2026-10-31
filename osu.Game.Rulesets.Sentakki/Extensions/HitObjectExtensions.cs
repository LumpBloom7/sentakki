using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Extensions;

public static class HitObjectExtensions
{
    extension(HitObject hitObject)
    {
        public Vector2 GetPosition()
            => ((IHasPosition)hitObject).Position;

        public Vector2 GetEndPosition()
        {
            var startPos = hitObject.GetPosition();

            if (hitObject is IHasPathWithRepeats slider)
                return startPos + slider.Path.PositionAt(1);

            return startPos;
        }
    }
}
