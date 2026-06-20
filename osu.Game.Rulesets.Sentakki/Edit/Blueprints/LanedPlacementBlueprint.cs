using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Types;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints;

public abstract partial class LanedPlacementBlueprint<T> : SentakkiPlacementBlueprint<T>
    where T : SentakkiHitObject, IHasLane, new()
{
    public override bool ReplacesExistingObject(HitObject existing)
        => base.ReplacesExistingObject(existing)
            && (existing is IHasLane lanedNote)
            && lanedNote.Lane == HitObject.Lane;
}
