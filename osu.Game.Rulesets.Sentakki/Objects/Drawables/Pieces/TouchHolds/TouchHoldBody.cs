using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds
{
    public partial class TouchHoldBody : CircularContainer
    {
        public readonly TouchHoldProgressPiece ProgressPiece;
        public readonly TouchHoldCentrePiece centrePiece;

        public readonly TouchHoldCompletedCentre CompletedCentre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => ProgressPiece.ReceivePositionalInputAt(screenSpacePos);

        public TouchHoldBody()
        {
            Size = new Vector2(130);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            InternalChildren = new Drawable[]
            {
                ProgressPiece = new TouchHoldProgressPiece(),
                centrePiece = new TouchHoldCentrePiece(),
                // We swap the centre piece with this other drawable to make it look better with the progress bar
                // Otherwise we'd need to add a thick border in between the centre and the progress
                CompletedCentre = new TouchHoldCompletedCentre(),
                new DotPiece(),
            };
        }
    }
}
