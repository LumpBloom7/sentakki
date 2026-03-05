using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;

public partial class HoldBody : CompositeDrawable
{
    // This will be proxied, so a must.
    public override bool RemoveWhenNotAlive => false;

    public override Quad ScreenSpaceDrawQuad => noteVisuals.ScreenSpaceDrawQuad;

    private readonly LaneNoteVisual noteVisuals;

    public HoldBody()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.TopCentre;
        InternalChildren =
        [
            new Container
            {
                // For simplicity in sizing and positioning
                // let's put the endpoints outside the main area
                Padding = new MarginPadding(-TapPiece.CIRCLE_RADIUS),
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = noteVisuals = new LaneNoteVisual
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
