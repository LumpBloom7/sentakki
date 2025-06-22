using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds;

public partial class TouchHoldCircularProgress : CircularProgress
{
    private Color4 originalColour;
    private Color4 flashingColour;

    public Color4 AccentColour
    {
        get => originalColour;
        set
        {
            originalColour = value;
            flashingColour = value.LightenHSL(0.4f);
        }
    }

    [Resolved]
    private Bindable<bool>? isHitting { get; set; }

    protected override void Update()
    {
        base.Update();

        if (isHitting?.Value ?? false)
        {
            const double flashing_time = 80;

            double flashProg = (Time.Current % (flashing_time * 2)) / (flashing_time * 2);

            if (flashProg <= 0.5)
                Colour = Interpolation.ValueAt(flashProg, originalColour, flashingColour, 0, 0.5, Easing.OutSine);
            else
                Colour = Interpolation.ValueAt(flashProg, flashingColour, originalColour, 0.5, 0, Easing.InSine);
        }
        else
        {
            Colour = originalColour;
        }
    }
}
