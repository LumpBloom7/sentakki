using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;

public partial class TapPlacementBlueprint : SentakkiPlacementBlueprint<Tap>
{
    private readonly TapHighlight highlight;

    [Resolved]
    private SentakkiHitObjectComposer composer { get; set; } = null!;

    [Resolved]
    private SentakkiSnapProvider snapProvider { get; set; } = null!;

    public TapPlacementBlueprint()
    {
        InternalChild = highlight = new TapHighlight();
        highlight.Y = -SentakkiPlayfield.INTERSECTDISTANCE;
    }

    protected override void Update()
    {
        Rotation = HitObject.Lane.GetRotationForLane();
        highlight.Y = -snapProvider.GetDistanceRelativeToCurrentTime(HitObject.StartTime, SentakkiPlayfield.NOTESTARTDISTANCE);
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button != MouseButton.Left) return false;

        EndPlacement(true);
        return true;
    }

    public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double fallbackTime)
    {
        var result = composer?.FindSnappedPositionAndTime(screenSpacePosition) ?? new SnapResult(screenSpacePosition, fallbackTime);

        base.UpdateTimeAndPosition(result.ScreenSpacePosition, result.Time ?? fallbackTime);

        if (result is not SentakkiLanedSnapResult senRes)
            return result;

        HitObject.Lane = senRes.Lane;
        highlight.Y = -senRes.YPos;

        return result;
    }
}
