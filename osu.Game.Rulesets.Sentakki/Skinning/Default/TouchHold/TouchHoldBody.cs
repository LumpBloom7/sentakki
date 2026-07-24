using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.Skinning.Common;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default.TouchHold;

public partial class TouchHoldBody : CircularContainer, IHasCopyableVisualState
{
    public readonly TouchHoldProgressPiece ProgressPiece;
    public readonly TouchHoldCentrePiece CentrePiece;

    public readonly TouchHoldCompletedCentre CompletedCentre;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => ProgressPiece.ReceivePositionalInputAt(screenSpacePos);

    public TouchHoldBody()
    {
        RelativeSizeAxes = Axes.Both;
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        InternalChildren =
        [
            ProgressPiece = new TouchHoldProgressPiece(),
            CentrePiece = new TouchHoldCentrePiece(),
            // We swap the centre piece with this other drawable to make it look better with the progress bar
            // Otherwise we'd need to add a thick border in between the centre and the progress
            CompletedCentre = new TouchHoldCompletedCentre(),
            new DotPiece()
        ];
    }

    [Resolved]
    private DrawableTouchHold? drawableTouchHold { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        if (drawableTouchHold is null)
            return;

        drawableTouchHold.ApplyCustomUpdateState += applyCustomUpdateState;
    }

    private void applyCustomUpdateState(DrawableHitObject hitobject, ArmedState state)
    {
        if (hitobject != drawableTouchHold)
            return;

        using (BeginAbsoluteSequence(drawableTouchHold.HitObject.StartTime))
        {
            ProgressPiece.TransformBindableTo(ProgressPiece.ProgressBindable, 1, drawableTouchHold.HitObject.Duration);
            CentrePiece.FadeOut();
            CompletedCentre.FadeIn();
        }
    }

    public void CopyVisualStateTo(IHasCopyableVisualState other)
    {
        if (other is not TouchHoldBody thb)
            return;

        thb.ProgressPiece.ProgressBindable.Value = ProgressPiece.ProgressBindable.Value;
        thb.CentrePiece.Alpha = CentrePiece.Alpha;
        thb.CompletedCentre.Alpha = CompletedCentre.Alpha;
    }
}
