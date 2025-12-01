using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class SentakkiSelectionHandler : EditorSelectionHandler
{
    public SentakkiSelectionHandler()
    {
        exTernaryState.ValueChanged += v => applyTernaryChanges<SentakkiHitObject>(setExState, v.NewValue);
        breakTernaryState.ValueChanged += v => applyTernaryChanges<SentakkiHitObject>(setBreakState, v.NewValue);

        exSlideTernaryState.ValueChanged += v => applyTernaryChanges<Slide>(setExSlideState, v.NewValue);
        breakSlideTernaryState.ValueChanged += v => applyTernaryChanges<Slide>(setBreakSlideState, v.NewValue);
    }

    #region ContextMenu

    protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<HitObject>> selection)
    {
        foreach (var item in base.GetContextMenuItemsForSelection(selection))
            yield return item;

        yield return new OsuMenuItemSpacer();

        var items = selection.Select(s => s.Item).OfType<SentakkiHitObject>();

        yield return new OsuMenuItem("Modifiers")
        {
            Items =
            [
                new TernaryStateToggleMenuItem("Break") { State = { BindTarget = breakTernaryState } },
                new TernaryStateToggleMenuItem("EX ") { State = { BindTarget = exTernaryState } }
            ]
        };

        var slideBodies = items.OfType<Slide>().SelectMany(s => s.SlideInfoList);

        if (!slideBodies.Any()) yield break;

        yield return new OsuMenuItem("Slide Modifiers")
        {
            Items =
            [
                new TernaryStateToggleMenuItem("Break") { State = { BindTarget = breakSlideTernaryState } },
                new TernaryStateToggleMenuItem("EX ") { State = { BindTarget = exSlideTernaryState } }
            ]
        };
    }

    private readonly Bindable<TernaryState> exTernaryState = new Bindable<TernaryState>();
    private readonly Bindable<TernaryState> breakTernaryState = new Bindable<TernaryState>();

    private readonly Bindable<TernaryState> exSlideTernaryState = new Bindable<TernaryState>();
    private readonly Bindable<TernaryState> breakSlideTernaryState = new Bindable<TernaryState>();

    protected override void UpdateTernaryStates()
    {
        var selectedItems = SelectedItems.OfType<SentakkiHitObject>();
        exTernaryState.Value = GetStateFromSelection(selectedItems, h => h.Ex);
        breakTernaryState.Value = GetStateFromSelection(selectedItems, h => h.Break);

        var selectedSlideBodies = selectedItems.OfType<Slide>().SelectMany(s => s.SlideInfoList);
        breakSlideTernaryState.Value = GetStateFromSelection(selectedSlideBodies, s => s.Break);
    }

    private void applyTernaryChanges<T>(Action<T, bool> applicator, TernaryState newTernaryState) where T : HitObject
    {
        var selectedItems = SelectedItems.OfType<T>();
        bool newValue = newTernaryState is TernaryState.True;

        EditorBeatmap.BeginChange();

        foreach (var item in selectedItems)
        {
            applicator(item, newValue);
            EditorBeatmap.Update(item);
        }

        EditorBeatmap.EndChange();
    }

    private void setExState(SentakkiHitObject hitObject, bool newValue) => hitObject.Ex = newValue;
    private void setBreakState(SentakkiHitObject hitObject, bool newValue) => hitObject.Break = newValue;

    private void setExSlideState(Slide slide, bool newValue) => slide.SlideInfoList.ForEach(si => si.Ex = newValue);
    private void setBreakSlideState(Slide slide, bool newValue) => slide.SlideInfoList.ForEach(si => si.Ex = newValue);

    #endregion

    #region ObjectMovement

    public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent)
    {
        var dragDelta = this.ScreenSpaceDeltaToParentSpace(moveEvent.ScreenSpaceDelta);
        moveTouchNotes(dragDelta);
        return true;
    }

    private void moveTouchNotes(Vector2 dragDelta)
    {
        const float boundary_radius = 270;

        List<IHasPosition> touches = [.. SelectedBlueprints.Select(bp => bp.Item).OfType<IHasPosition>()];

        if (touches.Count == 0)
            return;

        var centre = GeometryUtils.MinimumEnclosingCircle(touches).Item1;

        float minimalDragDistance = (centre + dragDelta).Length;
        var dragDirection = (dragDelta + centre).Normalized();

        foreach (var touch in touches)
        {
            // Get the relative position of the touch note, with the centre of mass as the origin.
            var positionAroundCenter = touch.Position - centre;

            // Project the relative position onto the drag direction
            // This gives us information about how far along the drag direction the touch note is.
            float b = (dragDirection.X * positionAroundCenter.X) + (dragDirection.Y * positionAroundCenter.Y);

            // This gives us the amount of violation incurred by the point. In our case
            float c = positionAroundCenter.LengthSquared - MathF.Pow(boundary_radius, 2);

            // b^2 is the squared projection of relative position onto the squared direction
            // c already measured the squared constraint violation
            // sqrt(b^2 - c) gives the intersect of the line from the origin/centre, which we offset with the projected relative position b
            float distanceToRingIntersect = MathF.Sqrt(MathF.Pow(b, 2) - c) - b;

            minimalDragDistance = Math.Min(minimalDragDistance, distanceToRingIntersect);
        }

        // No movement or invalid movement occurred, break out early to avoid NaN during normalisation.
        if (minimalDragDistance < 0) return;

        var movementAmount = -centre + (minimalDragDistance * dragDirection);

        EditorBeatmap.BeginChange();

        foreach (var touch in touches)
        {
            touch.Position += movementAmount;
            EditorBeatmap.Update((SentakkiHitObject)touch);
        }

        EditorBeatmap.EndChange();
    }

    #endregion
}
