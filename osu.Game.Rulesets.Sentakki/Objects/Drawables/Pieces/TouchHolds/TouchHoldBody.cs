using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds
{
    public partial class TouchHoldBody : CircularContainer
    {
        public readonly TouchHoldProgressPiece ProgressPiece;
        private readonly TouchHoldCentrePiece centrePiece;

        public readonly Container BorderContainer;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => ProgressPiece.ReceivePositionalInputAt(screenSpacePos);

        public TouchHoldBody()
        {
            Size = new Vector2(130);
            Scale = new Vector2(110f / 130f);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            InternalChildren = new Drawable[]
            {
                ProgressPiece = new TouchHoldProgressPiece(),
                centrePiece = new TouchHoldCentrePiece(),
                BorderContainer = new Container
                {

                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(90),
                    CornerRadius = 20f,
                    CornerExponent = 2.5f,
                    Rotation = 45,
                    Masking = true,
                    BorderThickness = 7f,
                    BorderColour = Color4.White,
                    Alpha = 0,
                    Child = new Box
                    {
                        Alpha = 0,
                        AlwaysPresent = true,
                        RelativeSizeAxes = Axes.Both
                    }
                },
            };
        }
    }
}
