using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds;

public partial class TouchHoldPlacementBlueprint : SentakkiPlacementBlueprint<TouchHold>
{
    [Cached]
    private IBindable<IReadOnlyList<Color4>>? paletteBindable { get; set; }
        = new Bindable<IReadOnlyList<Color4>>(TouchHoldSelectionBlueprint.SELECTION_PALETTE);

    private readonly TouchHoldBody highlight;

    public TouchHoldPlacementBlueprint()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        InternalChild = highlight = new TouchHoldBody
        {
            Alpha = 0.5f,
            Colour = Color4.YellowGreen
        };
    }

    protected override void Update()
    {
        base.Update();

        highlight.Position = HitObject.Position;
    }

    private double commitStartTime;

    [Resolved]
    private TouchPositionSnapGrid touchPositionSnapGrid { get; set; } = null!;

    public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double time)
    {
        switch (PlacementActive)
        {
            case PlacementState.Waiting:
                Vector2 localMousePosition = ToLocalSpace(screenSpacePosition) - OriginPosition;
                Vector2 snappedPos = touchPositionSnapGrid.GetSnappedPosition(localMousePosition);

                // Touch notes cannot be placed more than 270 units away from the centre
                float distance = Math.Min(snappedPos.Length, 270);

                Vector2 clampedPosition = distance == 0 ? Vector2.Zero : (snappedPos.Normalized() * distance);
                HitObject.Position = clampedPosition;

                break;

            case PlacementState.Active:
                HitObject.EndTime = Math.Max(commitStartTime, time);
                HitObject.StartTime = Math.Min(commitStartTime, time);

                break;
        }

        return base.UpdateTimeAndPosition(screenSpacePosition, time);
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button is not MouseButton.Left)
            return base.OnMouseDown(e);

        switch (PlacementActive)
        {
            case PlacementState.Waiting:
                if (!IsValidForPlacement)
                    break;

                BeginPlacement(true);
                commitStartTime = HitObject.StartTime;
                break;

            case PlacementState.Active:
                EndPlacement(true);
                break;
        }

        return true;
    }
}
