using System;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints;

public abstract partial class TouchPlacementBlueprint<T> : SentakkiPlacementBlueprint<T>
    where T : SentakkiHitObject, IHasPosition, new()
{
    private static readonly Lazy<float> minimum_touch_spacing = new Lazy<float>(getMinimumDistance);
    protected static float MinimumTouchSpacing => minimum_touch_spacing.Value;

    public override bool ReplacesExistingObject(HitObject existing)
        => base.ReplacesExistingObject(existing)
            && existing is IHasPosition touchNote
            && Vector2.Distance(HitObject.Position, touchNote.Position) < MinimumTouchSpacing;

    private static float getMinimumDistance()
    {
        var valid_positions = SentakkiBeatmapConverterOld.VALID_TOUCH_POSITIONS;
        int n = valid_positions.Count;

        float min_distance = float.MaxValue;
        for (int i = 0; i < n - 1; ++i)
        {
            var pos = valid_positions[i];
            for (int j = i + 1; j < n; ++j)
            {
                var pos2 = valid_positions[j];

                min_distance = Math.Min(min_distance, Vector2.Distance(pos, pos2));
            }
        }

        return min_distance;
    }
}
