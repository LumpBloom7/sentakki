using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints;

public partial class SentakkiPlacementBlueprint<T> : HitObjectPlacementBlueprint where T : SentakkiHitObject, new()
{
    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

    public new T HitObject => (T)base.HitObject;

    [Resolved]
    private SentakkiHitObjectComposer composer { get; set; } = null!;

    public SentakkiPlacementBlueprint()
        : base(new T())
    {
        Anchor = Origin = Anchor.Centre;
    }

    protected override void LoadComplete()
    {
        // To match behaviour of the hitsample ternary buttons
        HitObject.Break = composer.SelectionHandler.SelectionBreakState.Value is TernaryState.True;
        HitObject.Ex = composer.SelectionHandler.SelectionExState.Value is TernaryState.True;
        base.LoadComplete();
    }
}
