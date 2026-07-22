using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default.Touch;

public partial class TouchPieceShadow : CompositeDrawable
{
    public TouchPieceShadow()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        AddInternal(new TouchTriangle
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Size = new Vector2(75, 45f),
            Thickness = 15f,
            ShadowRadius = 15f,
            ShadowOnly = true,
        });
    }
}
