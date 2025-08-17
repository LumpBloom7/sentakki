using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touches;

public partial class TouchPlacementBlueprint : SentakkiPlacementBlueprint<Touch>
{
    private readonly TouchHighlight highlight;

    [Resolved]
    private SentakkiHitObjectComposer composer { get; set; } = null!;

    public TouchPlacementBlueprint()
    {
        InternalChild = highlight = new TouchHighlight();
    }

    protected override void Update()
    {
        highlight.Position = HitObject.Position;
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button != MouseButton.Left)
            return false;

        EndPlacement(true);
        return true;
    }

    public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double fallbackTime)
    {
        var result = composer?.FindSnappedPositionAndTime(screenSpacePosition) ?? new SnapResult(screenSpacePosition, fallbackTime);

        base.UpdateTimeAndPosition(result.ScreenSpacePosition, result.Time ?? fallbackTime);

        var newPosition = ToLocalSpace(result.ScreenSpacePosition) - OriginPosition;

        if (Vector2.Distance(Vector2.Zero, newPosition) > 270)
        {
            float angle = Vector2.Zero.AngleTo(newPosition);
            newPosition = MathExtensions.PointOnCircle(270, angle);
        }

        HitObject.Position = newPosition;

        return result;
    }
}
