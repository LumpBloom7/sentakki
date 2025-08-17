using System;
using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds;

public partial class HoldPlacementBlueprint : SentakkiPlacementBlueprint<Hold>
{
    protected override bool IsValidForPlacement => HitObject.Duration > 0;
    private readonly HoldHighlight highlight;

    [Resolved]
    private SentakkiHitObjectComposer composer { get; set; } = null!;

    public HoldPlacementBlueprint()
    {
        InternalChild = highlight = new HoldHighlight();
        highlight.Note.Y = -SentakkiPlayfield.INTERSECTDISTANCE;
    }

    [Resolved]
    private SentakkiSnapProvider snapProvider { get; set; } = null!;

    protected override void Update()
    {
        base.Update();
        highlight.Rotation = HitObject.Lane.GetRotationForLane();
        highlight.Note.Y = -snapProvider.GetDistanceRelativeToCurrentTime(HitObject.StartTime, SentakkiPlayfield.NOTESTARTDISTANCE);
        highlight.Note.Height = -snapProvider.GetDistanceRelativeToCurrentTime(HitObject.EndTime, SentakkiPlayfield.NOTESTARTDISTANCE) - highlight.Note.Y;
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button != MouseButton.Left)
            return false;

        if (PlacementActive == PlacementState.Active)
            return false;

        BeginPlacement(true);
        timeChanged = false;

        return true;
    }

    protected override void OnMouseUp(MouseUpEvent e)
    {
        if (PlacementActive != PlacementState.Active) return;

        if ((e.Button is MouseButton.Left && timeChanged) || e.Button is MouseButton.Right)
            EndPlacement(HitObject.Duration > 0);
    }

    private double originalStartTime;
    private bool timeChanged = false;

    public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double fallbackTime)
    {
        var result = composer?.FindSnappedPositionAndTime(screenSpacePosition) ?? new SnapResult(screenSpacePosition, fallbackTime);

        base.UpdateTimeAndPosition(result.ScreenSpacePosition, result.Time ?? fallbackTime);

        if (result is not SentakkiLanedSnapResult senRes)
            return result;

        double snapTime = result.Time ?? fallbackTime;

        if (PlacementActive == PlacementState.Active)
        {
            HitObject.StartTime = snapTime < originalStartTime ? snapTime : originalStartTime;
            HitObject.Duration = Math.Abs(snapTime - originalStartTime);

            if (HitObject.Duration > 0)
                timeChanged = true;
        }
        else
        {
            HitObject.Lane = senRes.Lane;
            originalStartTime = HitObject.StartTime = snapTime;
        }

        return result;
    }
}
