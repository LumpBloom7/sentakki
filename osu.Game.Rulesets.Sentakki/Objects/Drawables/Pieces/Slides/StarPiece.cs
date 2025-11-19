using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;

public partial class StarPiece : CompositeDrawable
{
    public StarPiece()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        Size = new Vector2(TapPiece.CIRCLE_RADIUS * 2);
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        AddRangeInternal([
            new LaneNoteVisual
            {
                RelativeSizeAxes = Axes.Both,

                Shape = NoteShape.Star,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }
        ]);
    }
}
