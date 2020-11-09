using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class TouchHoldBody : CircularContainer
    {
        public readonly TouchHoldProgressPiece ProgressPiece;
        private readonly FlashPiece flash;
        private readonly TouchHoldCentrePiece centrePiece;

        private readonly Drawable explode;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => centrePiece.ReceivePositionalInputAt(screenSpacePos);

        public TouchHoldBody()
        {
            Size = new Vector2(110);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            InternalChildren = new Drawable[]{
                flash = new FlashPiece{
                    Rotation = 45,
                    CornerExponent = 2.5f,
                    CornerRadius = 27.5f,
                },
                ProgressPiece = new TouchHoldProgressPiece(),
                centrePiece = new TouchHoldCentrePiece(),
                explode = new ExplodePiece(),
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
                explode.Colour = colour.NewValue;
            }, true);
        }

        private void updateState(ValueChangedEvent<ArmedState> state)
        {
            switch (state.NewValue)
            {
                case ArmedState.Hit:
                    const double flash_in = 40;
                    const double flash_out = 100;
                    flash.FadeTo(0.8f, flash_in)
                        .Then()
                        .FadeOut(flash_out);

                    explode.FadeIn(flash_in);
                    this.ScaleTo(1.5f, 400, Easing.OutQuad);

                    using (BeginDelayedSequence(flash_in, true))
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
