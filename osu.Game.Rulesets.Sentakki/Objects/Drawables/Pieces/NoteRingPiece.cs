using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    // This piece is used for laned notes, which share consistent elements
    // Each half is extends beyond the area of this drawable
    // The size property of this drawable affects the stretch of the ring
    public partial class NoteRingPiece : CompositeDrawable
    {
        private const float base_circle_size = 105f;

        public NoteRingPiece(bool hex = false)
        {
            Padding = new MarginPadding(-base_circle_size / 2);
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            InternalChildren = new Drawable[]
            {
                new RingNote(){
                    RelativeSizeAxes = Axes.Both,
                    Hex = hex,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
        }
    }
}
