using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints;

public partial class SentakkiSelectionBlueprint<T> : HitObjectSelectionBlueprint<T> where T : SentakkiHitObject
{
    protected override bool AlwaysShowWhenSelected => true;

    public SentakkiSelectionBlueprint(T hitObject)
        : base(hitObject)
    {
    }
}