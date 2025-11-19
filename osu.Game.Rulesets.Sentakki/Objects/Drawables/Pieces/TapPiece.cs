using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;

public partial class TapPiece : CompositeDrawable
{
    // This will be proxied, so a must.
    public override bool RemoveWhenNotAlive => false;

    public const float CIRCLE_RADIUS = 75 * 0.5f;

    public TapPiece()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        Size = new Vector2(CIRCLE_RADIUS * 2);
        InternalChild = new LaneNoteVisual()
        {
            RelativeSizeAxes = Axes.Both,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre
        };
    }

    private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

    [BackgroundDependencyLoader]
    private void load(DrawableHitObject drawableObject)
    {
        accentColour.BindTo(drawableObject.AccentColour);
        accentColour.BindValueChanged(colour => Colour = colour.NewValue, true);
    }
}
