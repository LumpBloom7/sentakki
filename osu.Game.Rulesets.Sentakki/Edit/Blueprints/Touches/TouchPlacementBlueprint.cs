using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Touches;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touches;

public partial class TouchPlacementBlueprint : SentakkiPlacementBlueprint<Touch>
{
    private readonly TouchBody highlight;

    public TouchPlacementBlueprint()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        InternalChild = highlight = new TouchBody()
        {
            Alpha = 0.5f,
            Colour = Color4.YellowGreen,
        };
    }

    protected override void Update()
    {
        base.Update();

        highlight.Position = HitObject.Position;
    }

    [Resolved]
    private TouchPositionSnapGrid touchPositionSnapGrid { get; set; } = null!;

    public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double time)
    {
        Vector2 localPos = ToLocalSpace(screenSpacePosition) - OriginPosition;

        Vector2? snappedPos = touchPositionSnapGrid.GetSnappedPosition(localPos);

        if (snappedPos.HasValue)
        {
            HitObject.Position = snappedPos.Value;
        }
        else
        {
            // Touch notes cannot be placed more than 270 units away from the centre
            float distance = Math.Min(localPos.Length, 270);

            Vector2 clampedPosition = localPos.Normalized() * distance;
            HitObject.Position = clampedPosition;
        }

        return base.UpdateTimeAndPosition(screenSpacePosition, time);
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button is not MouseButton.Left)
            return base.OnMouseDown(e);

        EndPlacement(true);
        return true;
    }
}
