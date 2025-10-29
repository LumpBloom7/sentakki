using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class TransformToolboxGroup : EditorToolboxGroup, IKeyBindingHandler<GlobalAction>
{
    private readonly BindableList<HitObject> selectedHitObjects = [];
    private readonly BindableBool canRotate = new BindableBool();

    private EditorToolButton rotateButton = null!;

    public SelectionRotationHandler RotationHandler { get; init; } = null!;

    public TransformToolboxGroup()
        : base("transform")
    {
    }

    [BackgroundDependencyLoader]
    private void load(EditorBeatmap editorBeatmap)
    {
        Child = new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Spacing = new Vector2(5),
            Children =
            [
                rotateButton = new EditorToolButton("Rotate",
                    () => new SpriteIcon { Icon = FontAwesome.Solid.Undo },
                    () => new PreciseRotationPopover(RotationHandler))
            ]
        };

        selectedHitObjects.BindTo(editorBeatmap.SelectedHitObjects);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        canRotate.BindTo(RotationHandler.CanRotateAroundPlayfieldOrigin);
        // bindings to `Enabled` on the buttons are decoupled on purpose
        // due to the weird `OsuButton` behaviour of resetting `Enabled` to `false` when `Action` is set.
        canRotate.BindValueChanged(rotate => rotateButton.Enabled.Value = rotate.NewValue, true);
    }

    public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
    {
        if (e.Repeat) return false;

        switch (e.Action)
        {
            case GlobalAction.EditorToggleRotateControl:
            {
                if (!RotationHandler.OperationInProgress.Value || rotateButton.Selected.Value)
                    rotateButton.TriggerClick();
                return true;
            }
        }

        return false;
    }

    public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
    {
    }
}
