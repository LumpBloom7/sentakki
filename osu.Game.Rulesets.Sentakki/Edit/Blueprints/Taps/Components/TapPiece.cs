using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK;

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
            BorderColour = Colour4.GreenYellow;
            BorderThickness = 3;

            InternalChild = new Box
            {
                Alpha = 0,
                RelativeSizeAxes = Axes.Both,
                AlwaysPresent = true
            };
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
    }
}