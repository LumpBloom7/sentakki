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
                new DotPiece(),
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
            Position = SentakkiExtensions.GetCircularPosition(-drawableTap.TapVisual.Position.Y, drawableTap.HitObject.Lane.GetRotationForLane());
        }
    }
}
