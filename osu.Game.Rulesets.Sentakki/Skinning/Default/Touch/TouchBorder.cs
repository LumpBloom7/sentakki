using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default.Touch;

public partial class TouchBorder : CompositeDrawable
{
    public TouchBorder()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
        CornerRadius = 15f;
        CornerExponent = 2.5f;
        Masking = true;
        BorderThickness = 8;
        BorderColour = Color4.White;
        Alpha = 0;
        InternalChild = new Box
        {
            Alpha = 0,
            AlwaysPresent = true,
            RelativeSizeAxes = Axes.Both
        };
    }
}
