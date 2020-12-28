using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osuTK;
using osuTK.Graphics;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Framework.Graphics.Primitives;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds.Components
{
    public class TouchHoldSelectionPiece : BlueprintPiece<TouchHold>
    {
        public Quad SelectionBoundaries => ScreenSpaceDrawQuad.AABBFloat.Shrink(10f);

        public TouchHoldSelectionPiece()
        {
            Size = new Vector2(110);
            Position = Vector2.Zero;

            CornerRadius = 27.5f;
            CornerExponent = 2.5f;
            Masking = true;
            BorderColour = Colour4.White;
            BorderThickness = 5;
            Rotation = 45;

            AddRangeInternal(new Drawable[]{
                new Box {
                    AlwaysPresent = true,
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both
                },
                new DotPiece(),
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }
    }
}
