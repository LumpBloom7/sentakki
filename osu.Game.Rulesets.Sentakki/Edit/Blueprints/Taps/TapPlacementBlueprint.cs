using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps;

public partial class TapPlacementBlueprint : SentakkiPlacementBlueprint<Tap>
{
    [Resolved]
    private LaneNoteSnapGrid snapGrid { get; set; } = null!;

    private readonly TapPiece highlight;

    public TapPlacementBlueprint()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        InternalChild = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Child = highlight = new TapPiece
            {
                Alpha = 0.5f,
                Colour = Color4.YellowGreen
            }
        };
    }

    private bool initialStateApplied;

    private readonly Bindable<double> animationSpeed = new Bindable<double>(5);

    [BackgroundDependencyLoader]
    private void load(SentakkiBlueprintContainer blueprintContainer)
    {
        animationSpeed.BindTo(blueprintContainer.Composer.DrawableRuleset.AdjustedAnimDuration);
    }

    protected override void Update()
    {
        base.Update();
        float newRotation = HitObject.Lane.GetRotationForLane();

        highlight.Y = -Interpolation.ValueAt(
            HitObject.StartTime,
            SentakkiPlayfield.INTERSECTDISTANCE,
            SentakkiPlayfield.NOTESTARTDISTANCE,
            EditorClock.CurrentTime,
            EditorClock.CurrentTime + animationSpeed.Value / 2
        );

        if (!initialStateApplied)
        {
            InternalChild.Rotation = newRotation;
            initialStateApplied = true;
        }

        float angleDelta = MathExtensions.AngleDelta(InternalChild.Rotation, newRotation);
        InternalChild.Rotation += 25 * angleDelta * (float)(Time.Elapsed / 1000);
    }

    public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double time)
    {
        (time, int lane) = snapGrid.GetSnappedTimeAndPosition(time, screenSpacePosition);

        HitObject.Lane = lane;

        return base.UpdateTimeAndPosition(screenSpacePosition, time);
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button != MouseButton.Left) return false;

        EndPlacement(true);
        return true;
    }
}
