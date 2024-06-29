using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides
{
    public partial class StarPiece : CompositeDrawable
    {
        private const float base_circle_size = 75;
        private const float drawable_size = base_circle_size + 30; // 30 units for shadow
        public StarPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Padding = new MarginPadding(-drawable_size / 2);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]{
                new RingNote{
                    RelativeSizeAxes = Axes.Both,

                    Shape = NoteShape.Star,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });
        }
    }
}
