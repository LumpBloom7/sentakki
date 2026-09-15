using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default;

public partial class HoldBody : CompositeDrawable
{
    public HoldBody()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.TopCentre;
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

    private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

    [BackgroundDependencyLoader]
    private void load(DrawableHitObject? drawableObject)
    {
        if (drawableObject is null)
            return;

        accentColour.BindTo(drawableObject.AccentColour);
        accentColour.BindValueChanged(colour => Colour = colour.NewValue, true);
    }
}
