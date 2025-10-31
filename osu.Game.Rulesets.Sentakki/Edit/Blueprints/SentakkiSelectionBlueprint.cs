using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints;

public abstract partial class SentakkiSelectionBlueprint<THitObject, TDrawable> : HitObjectSelectionBlueprint<THitObject>
    where THitObject : SentakkiHitObject
    where TDrawable : DrawableSentakkiHitObject
{
    public override Vector2 ScreenSpaceSelectionPoint => SelectionQuad.Centre;
    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => SelectionQuad.Contains(screenSpacePos);

    protected SentakkiSelectionBlueprint(THitObject item)
        : base(item)
    {
    }

    public new TDrawable DrawableObject => (TDrawable)base.DrawableObject;
}
