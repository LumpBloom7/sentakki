using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default;

public partial class HoldBody : CompositeDrawable
{
    public HoldBody()
    {
        RelativeSizeAxes = Axes.Both;
        InternalChildren =
        [
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = new LaneNoteVisual
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Shape = NoteShape.Hex
                }
            }
        ];
    }

    private readonly IBindable<Color4> accentColour = new Bindable<Color4>(Color4.White);

    private readonly Bindable<bool> isHitting = new Bindable<bool>();

    private Color4 flashingColour = Color4.White;

    [BackgroundDependencyLoader]
    private void load(DrawableHitObject? drawableObject)
    {
        if (drawableObject is not DrawableHold drawableHold)
            return;

        accentColour.BindTo(drawableHold.AccentColour);
        accentColour.BindValueChanged(c => flashingColour = c.NewValue.LightenHsl(0.4f), true);

        isHitting.BindTo(drawableHold.IsHitting);
    }

    protected override void Update()
    {
        base.Update();

        if (!isHitting.Value)
        {
            Colour = accentColour.Value;
            return;
        }

        const double flashing_time = 80;

        double flashProg = Time.Current % (flashing_time * 2) / (flashing_time * 2);

        if (flashProg <= 0.5)
            Colour = Interpolation.ValueAt(flashProg, accentColour.Value, flashingColour, 0, 0.5, Easing.OutSine);
        else
            Colour = Interpolation.ValueAt(flashProg, flashingColour, accentColour.Value, 0.5, 0, Easing.InSine);
    }
}
