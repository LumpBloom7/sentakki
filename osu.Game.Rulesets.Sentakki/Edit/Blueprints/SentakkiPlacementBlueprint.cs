using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints;

public partial class SentakkiPlacementBlueprint<T> : HitObjectPlacementBlueprint where T : SentakkiHitObject, new()
{
    public new T HitObject => (T)base.HitObject;

    public SentakkiPlacementBlueprint()
        : base(new T())
    {
    }
}
