using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints;

public abstract partial class SentakkiPlacementBlueprint<T> : HitObjectPlacementBlueprint where T : SentakkiHitObject, new()
{
    public new T HitObject => (T)base.HitObject;

    [Resolved]
    private SentakkiHitObjectComposer composer { get; set; } = null!;

    public SentakkiPlacementBlueprint()
        : base(new T())
    {
    }

    public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double time)
    {
        HitObject.Break = composer.BreakTernaryState.Value is TernaryState.True;
        HitObject.Ex = composer.ExTernaryState.Value is TernaryState.True;

        return base.UpdateTimeAndPosition(screenSpacePosition, time);
    }
}
