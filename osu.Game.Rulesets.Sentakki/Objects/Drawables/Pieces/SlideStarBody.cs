using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class SlideStarBody : CompositeDrawable
    {
        private readonly StarPiece star;
        //private readonly FlashPiece flash;
        private readonly ExplodePiece explode;
        //private readonly ShadowPiece glow;

        public SlideStarBody()
        {
            Size = new Vector2(80);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                //glow = new ShadowPiece(),
                star = new StarPiece(),
                //flash = new FlashPiece(),
                explode = new ExplodePiece(),
            };
        }

        private readonly IBindable<ArmedState> state = new Bindable<ArmedState>();
        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            Tap osuObject = (Tap)drawableObject.HitObject;

            state.BindTo(drawableObject.State);
            state.BindValueChanged(updateState, true);

            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour =>
            {
                explode.Colour = colour.NewValue;
                star.Colour = colour.NewValue;
            }, true);
        }

        private void updateState(ValueChangedEvent<ArmedState> state)
        {
            //glow.FadeOut(400);

            switch (state.NewValue)
            {
                case ArmedState.Hit:
                    const double flash_in = 40;
                    //const double flash_out = 100;

                    explode.FadeIn(flash_in);
                    this.ScaleTo(1.5f, 400, Easing.OutQuad);

                    using (BeginDelayedSequence(flash_in, true))
                    {
                        //after the flash, we can hide some elements that were behind it
                        star.FadeOut();
                        //glow.FadeOut();

                        this.FadeOut(800);
                    }

                    break;
            }
        }
    }
}
