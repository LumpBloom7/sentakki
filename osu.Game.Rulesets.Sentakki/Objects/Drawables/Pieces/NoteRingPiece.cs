using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;

// This piece is used for laned notes, which share consistent elements
// Each half is extends beyond the area of this drawable
// The size property of this drawable affects the stretch of the ring
public partial class NoteRingPiece : CompositeDrawable
{
    private const float base_circle_size = 75;
    public const float DRAWABLE_SIZE = base_circle_size + 30; // 30 units for shadow

    public NoteRingPiece(bool hex = false)
    {
        Padding = new MarginPadding(-DRAWABLE_SIZE / 2);
        RelativeSizeAxes = Axes.Both;
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        InternalChildren =
        [
            new LaneNoteVisual
            {
                RelativeSizeAxes = Axes.Both,
                Shape = hex ? NoteShape.Hex : NoteShape.Ring,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }
        ];
    }
}
