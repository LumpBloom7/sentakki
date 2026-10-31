using System;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints;

public abstract partial class TouchPlacementBlueprintBase<T> : SentakkiPlacementBlueprint<T>
    where T : SentakkiHitObject, IHasPosition, new()
{
    protected static float MinimumTouchSpacing => TouchPlacementBlueprintBaseShared.MINIMUM_TOUCH_SPACING;

    public override bool ReplacesExistingObject(HitObject existing)
        => base.ReplacesExistingObject(existing)
           && existing is IHasPosition touchNote
           && Precision.DefinitelyBigger(MinimumTouchSpacing, Vector2.Distance(HitObject.Position, touchNote.Position));
}

internal class TouchPlacementBlueprintBaseShared
{
    public static readonly float MINIMUM_TOUCH_SPACING;

    private TouchPlacementBlueprintBaseShared() { }

    static TouchPlacementBlueprintBaseShared()
    {
        MINIMUM_TOUCH_SPACING = getMinimumDistance();
    }

    private static float getMinimumDistance()
    {
        var validPositions = SentakkiBeatmapConverterOld.VALID_TOUCH_POSITIONS;
        int n = validPositions.Count;

        float minDistanceSquared = float.MaxValue;
        for (int i = 0; i < n - 1; ++i)
        {
            var pos = validPositions[i];
            for (int j = i + 1; j < n; ++j)
            {
                var pos2 = validPositions[j];

                minDistanceSquared = Math.Min(minDistanceSquared, Vector2.DistanceSquared(pos, pos2));
            }
        }

        return MathF.Sqrt(minDistanceSquared);
    }
}