using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Taps.Components
{
    public class TapPiece : BlueprintPiece<Tap>
    {
        public TapPiece()
        {
            Size = new Vector2(80);

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
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }

        public override void UpdateFrom(Tap hitObject)
        {
            base.UpdateFrom(hitObject);
        }

        public void UpdateFrom(DrawableTap drawableTap)
        {
            Position = drawableTap.CirclePiece.Position;
        }
    }
}
