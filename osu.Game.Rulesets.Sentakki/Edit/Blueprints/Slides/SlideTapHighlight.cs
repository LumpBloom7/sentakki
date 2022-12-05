using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides
{
    public partial class SlideTapHighlight : CompositeDrawable
    {
        public readonly SlideTapPiece SlideTapPiece;

        // This drawable is zero width
        // We should use the quad of the note container
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => SlideTapPiece.ReceivePositionalInputAt(screenSpacePos);
        public override Quad ScreenSpaceDrawQuad => SlideTapPiece.ScreenSpaceDrawQuad;

        public SlideTapHighlight()
        {
            Anchor = Origin = Anchor.Centre;
            Colour = Color4.YellowGreen;
            Alpha = 0.5f;
            InternalChild = SlideTapPiece = new SlideTapPiece();
        }
    }
}
