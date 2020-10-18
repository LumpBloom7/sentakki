using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class HoldBody : CompositeDrawable
    {
        // This will be proxied, so a must.
        public override bool RemoveWhenNotAlive => false;

        private readonly FlashPiece flash;
        private readonly ExplodePiece explode;
        public readonly Container Note;
        private readonly ShadowPiece shadow;

        public double Duration = 0;

        public HoldBody()
        {
            Scale = Vector2.Zero;
            Position = new Vector2(0, -(SentakkiPlayfield.NOTESTARTDISTANCE - 37.5f));
            Anchor = Anchor.Centre;
            Origin = Anchor.BottomCentre;
            Size = new Vector2(75);
            InternalChildren = new Drawable[]
            {
                Note = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes=Axes.Both,
                    Children = new Drawable[]
                    {
                        shadow = new ShadowPiece(),
                        new RingPiece(),
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
                        },
                    }
                },
                flash = new FlashPiece(){
                    CornerRadius = 40,
                    CornerExponent = 2
                },
                explode = new ExplodePiece()
            };
        }

        private readonly IBindable<ArmedState> state = new Bindable<ArmedState>();
        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            state.BindTo(drawableObject.State);
            state.BindValueChanged(updateState, true);

            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour =>
            {
                explode.Colour = colour.NewValue;
                Note.Colour = colour.NewValue;
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

                    using (BeginDelayedSequence(Duration, true))
                    {
                        explode.FadeIn(flash_in);
                        this.ScaleTo(1.5f, 400, Easing.OutQuad);

                        using (BeginDelayedSequence(flash_in, true))
                        {
                            shadow.FadeOut();
                            Note.FadeOut();
                            this.FadeOut(800);
                        }
                    }
                    break;
            }
        }
    }
}
