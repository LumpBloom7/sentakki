using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds
{
    public partial class HoldHighlight : CompositeDrawable
    {
        public readonly Container Note;

        // This drawable is zero width
        // We should use the quad of the note container
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => ring.ScreenSpaceDrawQuad.Contains(screenSpacePos);
        public override Quad ScreenSpaceDrawQuad => ring.ScreenSpaceDrawQuad;

        private readonly NoteRingPiece ring;

        public HoldHighlight()
        {
            Anchor = Origin = Anchor.Centre;
            Colour = Color4.YellowGreen;
            Alpha = 0.5f;
            InternalChildren = new Drawable[]
            {
                Note = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Child = ring = new NoteRingPiece(true)
                        },
                    }
                }
            };
        }
    }
}
