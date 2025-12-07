using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;
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

    private readonly Bindable<TernaryState> breakState = new Bindable<TernaryState>();
    private readonly Bindable<TernaryState> exState = new Bindable<TernaryState>();

    [BackgroundDependencyLoader]
    private void load(SentakkiBlueprintContainer blueprintContainer)
    {
        var selectionHandler = (SentakkiSelectionHandler)blueprintContainer.SelectionHandler;

        // We want to inherit the current state of the flag ternary buttons, similarly to hitobject samples.
        // The samples are being set by `ComposeBlueprintContainer.ensurePlacementCreated`, which is not overridable.
        // So we need to implement similar logic... here...
        breakState.BindTo(selectionHandler.BreakTernaryState);
        exState.BindTo(selectionHandler.ExTernaryState);

        breakState.BindValueChanged(v => HitObject.Break = v.NewValue == TernaryState.True, true);
        exState.BindValueChanged(v => HitObject.Ex = v.NewValue == TernaryState.True, true);
    }
}
