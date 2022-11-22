using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit
{
    public class SentakkiSelectionHandler : EditorSelectionHandler
    {
        private readonly Bindable<TernaryState> selectionBreakState = new Bindable<TernaryState>();

        public SentakkiSelectionHandler()
        {
            selectionBreakState.ValueChanged += s =>
            {
                switch (s.NewValue)
                {
                    case TernaryState.False:
                    case TernaryState.True:
                        setBreakState(s.NewValue == TernaryState.True);
                        break;
                }
            };
        }

        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent)
        {
            // The lanes are arranged in a circular fashion
            // Blindly deriving new angle by adding the vector to each selection point will yield unintuitive results, such as all notes moving towards the drag direction
            //
            // Instead, we should derive an angle delta by comparing the current mouse position and the drag origin
            // The drag origin is the blueprint most closest to the mouse during the beginning of the drag, rather than the earliest note (see "SentakkiBlueprintContainer.SortForMovement")
            // This allows a more intuitive "steering wheel" like lane adjustments
            if (SelectedBlueprints.All(bp => bp.Item is SentakkiLanedHitObject))
            {
                var oldPosition = moveEvent.Blueprint.ScreenSpaceSelectionPoint;
                var newPosition = moveEvent.Blueprint.ScreenSpaceSelectionPoint + moveEvent.ScreenSpaceDelta;
                var playfieldCentre = ToScreenSpace(new Vector2(300));
                var angleDelta = playfieldCentre.GetDegreesFromPosition(newPosition) - playfieldCentre.GetDegreesFromPosition(oldPosition);

                foreach (var bp in SelectedBlueprints.ToList())
                {
                    var laned = (SentakkiLanedHitObject)bp.Item;
                    var currentAngle = laned.Lane.GetRotationForLane() + angleDelta;
                    laned.Lane = currentAngle.GetNoteLaneFromDegrees();
                }
                return true;
            }
            else if (SelectedBlueprints.All(bp => bp.Item is Touch))
            {
                // Special movement handling to ensure that all touch notes are within 250 units from the playfield centre
                moveTouchNotes(this.ScreenSpaceDeltaToParentSpace(moveEvent.ScreenSpaceDelta));
                return true;
            }
            return false;
        }

        private void setBreakState(bool state)
        {
            var lhos = EditorBeatmap.SelectedHitObjects.OfType<SentakkiLanedHitObject>();

            EditorBeatmap.BeginChange();

            foreach (var lho in lhos)
            {
                if (lho.Break == state)
                    continue;

                lho.Break = state;
                EditorBeatmap.Update(lho);
            }

            EditorBeatmap.EndChange();
        }

        protected override void UpdateTernaryStates()
        {
            base.UpdateTernaryStates();

            selectionBreakState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<SentakkiLanedHitObject>(), h => h.Break);
        }

        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<HitObject>> selection)
        {
            if (selection.Any(s => s.Item is SentakkiLanedHitObject))
                yield return new TernaryStateToggleMenuItem("Break") { State = { BindTarget = selectionBreakState } };

            foreach (var item in base.GetContextMenuItemsForSelection(selection))
                yield return item;
        }

        private void moveTouchNotes(Vector2 dragDelta)
        {
            const float boundary_radius = 250;

            float dragDistance(Vector2 origin, Vector2 destination)
                => MathF.Min((destination - origin).Length, circleIntersectionDistance(origin, destination - origin));

            float circleIntersectionDistance(Vector2 centre, Vector2 direction)
            {
                direction.Normalize();
                var b = (direction.X * centre.X) + (direction.Y * centre.Y);
                var c = centre.LengthSquared - (boundary_radius * boundary_radius);
                return MathF.Sqrt((b * b) - c) - b;
            }

            var touches = SelectedBlueprints.Select(bp => (Touch)bp.Item).ToList();
            var centre = touches.Aggregate(Vector2.Zero, (a, b) => a + b.Position) / touches.Count;
            var cappedDragDelta = touches.Min(t => dragDistance(t.Position - centre, t.Position + dragDelta));

            if (!(cappedDragDelta >= 0)) return; // No movement or invalid movement occurred

            foreach (var touch in touches)
                touch.Position = touch.Position - centre + (cappedDragDelta * (dragDelta + centre).Normalized());
        }
    }
}
