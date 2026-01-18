using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit;

public partial class SentakkiMovementHandler : Component
{
    public SentakkiMovementHandler()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
    }

    [Resolved]
    private TouchPositionSnapGrid touchSnapGrid { get; set; } = null!;

    [Resolved]
    private LaneNoteSnapGrid laneNoteSnapGrid { get; set; } = null!;

    [Resolved]
    private EditorBeatmap editorBeatmap { get; set; } = null!;

    [Resolved]
    private SentakkiHitObjectComposer composer { get; set; } = null!;

    public bool TryMoveBlueprints(DragEvent e, IList<(SelectionBlueprint<HitObject> blueprint, Vector2[] originalSnapPositions)> blueprints)
    {
        Vector2 dragVector = e.ScreenSpaceMousePosition - e.ScreenSpaceMouseDownPosition;

        var referenceObject = blueprints.First();

        Vector2 unsnappedPosition = referenceObject.originalSnapPositions[0] + dragVector;

        var localSpaceOriginalPosition = ToLocalSpace(referenceObject.blueprint.ScreenSpaceSelectionPoint) - OriginPosition;
        var localSpaceUnsnappedTarget = ToLocalSpace(unsnappedPosition) - OriginPosition;

        if (blueprints.All(bp => bp.blueprint.Item is IHasPosition))
            return tryPerformTouchMovement(localSpaceUnsnappedTarget, localSpaceOriginalPosition);
        else
            return tryPerformLaneMovement(localSpaceUnsnappedTarget, localSpaceOriginalPosition, referenceObject.blueprint.Item.StartTime);
    }

    private bool tryPerformLaneMovement(Vector2 localTarget, Vector2 localPosition, double originalTime)
    {
        (double snappedTime, _) = laneNoteSnapGrid.GetSnappedTimeAndPosition(originalTime, localTarget);

        float originalAngle = Vector2.Zero.AngleTo(localPosition);
        float currentAngle = Vector2.Zero.AngleTo(localTarget);

        float angleDelta = MathExtensions.AngleDelta(originalAngle, currentAngle);
        int laneOffset = (int)Math.Round(angleDelta / 45);

        double timeOffset = snappedTime - originalTime;

        // In order to prevent accidentally changing the time when the intention was to rotate
        // we make sure the resulting time of the notes don't jump significantly
        // Add 1ms leniency to account for slight unsnapping of notes
        if (Precision.DefinitelyBigger(Math.Abs(timeOffset), editorBeatmap.GetBeatLengthAtTime(snappedTime), 1))
            timeOffset = 0;

        if (laneOffset == 0 && timeOffset == 0)
            return false;

        editorBeatmap.BeginChange();

        var playfield = (SentakkiPlayfield)composer.Playfield;

        foreach (var lanedNote in editorBeatmap.SelectedHitObjects.OfType<SentakkiLanedHitObject>())
        {
            playfield.Remove(lanedNote);
            lanedNote.Lane = (lanedNote.Lane + laneOffset).NormalizeLane();
            lanedNote.StartTime += timeOffset;
            editorBeatmap.Update(lanedNote);
            playfield.Add(lanedNote);
        }

        var touchNotes = editorBeatmap.SelectedHitObjects.OfType<IHasPosition>();

        // Rotate the touch notes around the origin, in order to preserve their relative positions to the laned notes.
        if (touchNotes.Any())
        {
            float theta = laneOffset * 45f / 180 * MathF.PI;
            (float sin, float cos) = MathF.SinCos(theta);

            foreach (var touchNote in touchNotes)
            {
                var pos = touchNote.Position;

                touchNote.Position = new Vector2
                {
                    X = cos * pos.X - sin * pos.Y,
                    Y = sin * pos.X + cos * pos.Y
                };

                ((SentakkiHitObject)touchNote).StartTime += timeOffset;

                editorBeatmap.Update((SentakkiHitObject)touchNote);
            }
        }

        editorBeatmap.EndChange();

        return true;
    }

    private bool tryPerformTouchMovement(Vector2 localTarget, Vector2 localPosition)
    {
        const float boundary_radius = 270;

        Vector2 movementAmount;

        List<IHasPosition> touches = [.. editorBeatmap.SelectedHitObjects.OfType<IHasPosition>()];

        if (touchSnapGrid.State.Value is Visibility.Visible)
        {
            // When the snap grid is visible, any attempts to move a note will be snapped to the dot.
            // Only the note being targetted needs to be snapped.
            // All notes must be within the playfield boundaries for movement to be considered valid

            var snappedTarget = touchSnapGrid.GetSnappedPosition(localTarget);
            var localSpaceDelta = snappedTarget - localPosition;

            // Pre-check to determine whether it is worth attempting the movement
            foreach (var touchNotes in touches)
            {
                var snappedPosition = touchNotes.Position + localSpaceDelta;

                if (Precision.DefinitelyBigger(snappedPosition.Length, boundary_radius))
                    return false;
            }

            movementAmount = localSpaceDelta;
        }
        else
        {
            var localSpaceDelta = localTarget - localPosition;

            var centre = GeometryUtils.MinimumEnclosingCircle(touches).Item1;

            float minimalDragDistance = (centre + localSpaceDelta).Length;
            var dragDirection = (localSpaceDelta + centre).Normalized();

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

                // If the note is already outside of the ring, then any movement is invalid
                if (distanceToRingIntersect <= 0)
                    return false;

                minimalDragDistance = Math.Min(minimalDragDistance, distanceToRingIntersect);
            }

            movementAmount = -centre + (minimalDragDistance * dragDirection);
        }

        editorBeatmap.BeginChange();

        // Apply the movement to each touch note
        foreach (var touchNote in touches)
        {
            touchNote.Position += movementAmount;

            editorBeatmap.Update((HitObject)touchNote);
        }

        editorBeatmap.EndChange();
        return true;
    }
}