using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.Skinning.Default.Touch;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds;

public partial class TouchHoldPiece : CompositeDrawable
{
    public TouchHoldPiece()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        AddInternal(new TouchTriangle
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Size = new Vector2(75, 45f),
            Thickness = 15f,
            ShadowRadius = 0f,
            FillTriangle = true,
        });
    }
}
