using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Touchs.Components
{
    public class TouchSelectionPiece : BlueprintPiece<Touch>
    {
        public TouchSelectionPiece()
        {
            Size = new Vector2(80);

            CornerRadius = 20;
            CornerExponent = 2.5f;
            Masking = true;
            BorderColour = Colour4.White;
            BorderThickness = 5;

            InternalChildren = new Drawable[]{
                new Box{
                    Alpha = 0,
                    AlwaysPresent = true,
                    RelativeSizeAxes = Axes.Both
                },
                new CircularContainer
                {
                    Size = new Vector2(20),
                    Masking = true,
                    BorderColour = Color4.Gray,
                    BorderThickness = 2,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        AlwaysPresent = true,
                        Colour = Color4.White,
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }

        public override void UpdateFrom(Touch hitObject)
        {
            base.UpdateFrom(hitObject);
            Position = hitObject.Position;
        }

        public void UpdateFrom(DrawableTouch drawableTouch)
        {
            Position = drawableTouch.Position;
        }
    }
}
