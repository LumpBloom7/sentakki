using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds
{
    public partial class TouchHoldHighlight : TouchHoldBody
    {
        public override Quad ScreenSpaceDrawQuad => ProgressPiece.ScreenSpaceDrawQuad;

        public TouchHoldHighlight()
        {
            Anchor = Origin = Anchor.Centre;
            Colour = Color4.YellowGreen;
            Alpha = 0.5f;
        }
    }
}
