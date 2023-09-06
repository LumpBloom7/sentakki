using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds
{
    public partial class TouchHoldBody : CircularContainer
    {
        public readonly TouchHoldProgressPiece ProgressPiece;
        private readonly TouchHoldCentrePiece centrePiece;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => centrePiece.ReceivePositionalInputAt(screenSpacePos);

        public TouchHoldBody()
        {
            Size = new Vector2(110);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            InternalChildren = new Drawable[]
            {
                ProgressPiece = new TouchHoldProgressPiece(),
                centrePiece = new TouchHoldCentrePiece(),
            };
        }
    }
}
