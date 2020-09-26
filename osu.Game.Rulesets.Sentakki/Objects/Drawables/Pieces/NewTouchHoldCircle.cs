using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Effects;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class NewTouchHoldCircle : CircularContainer
    {
        public readonly TouchHoldProgressPiece ProgressPiece;
        private readonly FlashPiece flash;
        private readonly TouchHoldCentrePiece centrePiece;

        public double Duration;
        public NewTouchHoldCircle()
        {
            RelativeSizeAxes = Axes.Both;
            Size = Vector2.One;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            InternalChildren = new Drawable[]{
                flash = new FlashPiece{
                    Rotation = 45,
                    CornerExponent = 2.5f,
                    CornerRadius = 27.5f,
                },
                ProgressPiece = new TouchHoldProgressPiece(),
                centrePiece = new TouchHoldCentrePiece()
            };
        }

        private readonly IBindable<ArmedState> state = new Bindable<ArmedState>();
        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, DrawableHitObject drawableObject)
        {
            state.BindTo(drawableObject.State);
            state.BindValueChanged(updateState, true);

            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour =>
            {
            }, true);
        }

        private void updateState(ValueChangedEvent<ArmedState> state)
        {
            switch (state.NewValue)
            {
                case ArmedState.Hit:
                    const double flash_in = 40;
                    const double flash_out = 100;

                    flash.Delay(Duration).FadeTo(0.8f, flash_in)
                          .Then()
                          .FadeOut(flash_out);

                    // explode.Delay(Duration).FadeIn(flash_in);
                    this.Delay(Duration).ScaleTo(1.5f, 400, Easing.OutQuad);

                    using (BeginDelayedSequence(Duration + flash_in, true))
                    {
                        //after the flash, we can hide some elements that were behind it
                        ProgressPiece.FadeOut();
                        centrePiece.FadeOut();

                        this.FadeOut(800);
                    }
                    break;
            }
        }
    }
}
