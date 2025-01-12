using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides
{
    public partial class StarPiece : CompositeDrawable
    {
        private const float base_circle_size = 75;
        private const float drawable_size = base_circle_size + 30; // 30 units for shadow

        private LaneNoteVisual visual = null!;

        // We don't want to include the area where the shadow would be
        public override Quad ScreenSpaceDrawQuad => visual.ConservativeScreenSpaceDrawQuad;
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => ScreenSpaceDrawQuad.Contains(screenSpacePos);

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
                visual = new LaneNoteVisual{
                    RelativeSizeAxes = Axes.Both,

                    Shape = NoteShape.Star,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });
        }
    }
}
