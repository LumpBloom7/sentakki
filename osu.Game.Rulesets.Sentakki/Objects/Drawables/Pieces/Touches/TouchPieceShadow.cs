using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Touches
{
    public partial class TouchPieceShadow : CompositeDrawable
    {
        public TouchPieceShadow()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AddInternal(new DrawableTouchTriangle
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Size = new Vector2(103, 75f),
                Thickness = 8f,
                ShadowRadius = 15,
                ShadowOnly = true,
                Y = -15
            });
        }
    }
}
