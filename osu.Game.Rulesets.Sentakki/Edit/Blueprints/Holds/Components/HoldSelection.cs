using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Holds.Components
{
    public class HoldSelection : BlueprintPiece<Hold>
    {
        // This needs to be shrunk because the AABB box has larger margins for some reason
        public Quad SelectionBoundaries => notebody.ScreenSpaceDrawQuad.AABBFloat.Shrink(10f);
        private Container notebody;
        public HoldSelection()
        {
            InternalChildren = new Drawable[]{
                notebody = new CircularContainer{
                    Position = new Vector2(0, -(SentakkiPlayfield.NOTESTARTDISTANCE - 37.5f)),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    Size = new Vector2(75),
                    Masking = true,
                    BorderColour = Colour4.White,
                    BorderThickness = 5,
                    Children = new Drawable[]{
                        new Box{
                            AlwaysPresent = true,
                            Alpha = 0,
                            RelativeSizeAxes = Axes.Both
                        },
                        new DotPiece(squared: true)
                        {
                            Rotation = 45,
                            Position = new Vector2(0, -37.5f),
                            Anchor = Anchor.BottomCentre,
                        },
                        new DotPiece(squared: true)
                        {
                            Rotation = 45,
                            Position = new Vector2(0, 37.5f),
                            Anchor = Anchor.TopCentre,
                        }
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }

        public override void UpdateFrom(Hold hitObject)
        {
            //base.UpdateFrom(hitObject);
            Rotation = hitObject.Lane.GetRotationForLane();
            notebody.Position = new Vector2(0, -SentakkiPlayfield.INTERSECTDISTANCE + 37.5f);
        }

        public void UpdateFrom(DrawableHold drawableHold)
        {
            notebody.Position = drawableHold.NoteBody.Position;
            notebody.Height = drawableHold.NoteBody.Height;
            Rotation = drawableHold.HitObject.Lane.GetRotationForLane();
        }
    }
}
