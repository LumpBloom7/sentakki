using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Touches
{
    public partial class TouchPiece : CompositeDrawable
    {
        public TouchPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AddInternal(new DrawableTouchTriangle
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Size = new Vector2(73, 45f),
                Thickness = 8f,
                ShadowRadius = 0f,
            });
        }
    }
}
