using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets.Sentakki.Extensions;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds;

public partial class TouchHoldCircularProgress : CircularProgress
{
    private readonly Color4 originalColour;
    private readonly Color4 flashingColour;

    public TouchHoldCircularProgress(Color4 colour)
    {
        Colour = originalColour = colour;
        flashingColour = colour.Lighten(0.4f);
    }

    private Bindable<bool> isHitting { get; set; } = new();

    [BackgroundDependencyLoader]
    private void load(Bindable<bool>? isHittingBindable)
    {
        isHitting.TryBindTo(isHittingBindable);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        isHitting.BindValueChanged(hitting =>
        {
            const float animation_length = 80;

            ClearTransforms();

            if (hitting.NewValue)
            {
                // wait for the next sync point
                double synchronisedOffset = animation_length * 2 - Time.Current % (animation_length * 2);

                using (BeginDelayedSequence(synchronisedOffset))
                {
                    this.FadeColour(flashingColour, animation_length, Easing.OutSine).Then()
                        .FadeColour(originalColour, animation_length, Easing.InSine)
                        .Loop();
                }
            }
            else
            {
                this.FadeColour(originalColour);
            }
        }, true);
    }
}
