using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.TouchHolds.Components
{
    public class TouchHoldSelectionPiece : BlueprintPiece<TouchHold>
    {
        public TouchHoldSelectionPiece()
        {
            Size = new Vector2(120);
            Position = Vector2.Zero;

            CornerRadius = Size.X / 2;
            CornerExponent = 2;
            Masking = true;
            BorderColour = Colour4.White;
            BorderThickness = 5;

            AddRangeInternal(new Drawable[]{
                new Box{
                    AlwaysPresent = true,
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both
                },
                new CircularContainer
                {
                    Size = new Vector2(92),
                    Masking = true,
                    BorderColour = Color4.White,
                    BorderThickness = 2,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }
    }
}
