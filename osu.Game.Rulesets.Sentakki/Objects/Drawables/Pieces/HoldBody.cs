using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class HoldBody : CompositeDrawable
    {
        private readonly FlashPiece flash;
        private readonly ExplodePiece explode;
        private readonly Container note;
        public readonly GlowPiece Glow;

        public double Duration = 0;

        public HoldBody()
        {
            Scale = Vector2.Zero;
            Position = new Vector2(0, -26);
            Anchor = Anchor.Centre;
            Origin = Anchor.BottomCentre;
            Size = new Vector2(80);
            InternalChildren = new Drawable[]
            {
                note = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes=Axes.Both,
                    Children = new Drawable[]
                    {
                        Glow = new GlowPiece(){
                            Alpha = 0,
                        },
                        new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            BorderThickness = 17.35f,
                            BorderColour = Color4.Gray,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true,
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(1),
                            Child = new CircularContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                BorderThickness = 15,
                                BorderColour = Color4.White,
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                    AlwaysPresent = true,
                                }
                            }
                        },
                        new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            BorderThickness = 2,
                            BorderColour = Color4.Gray,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true,
                            }
                        },
                        new CircularContainer
                        {
                            Position = new Vector2(0, -40),
                            Size = new Vector2(20),
                            Masking = true,
                            BorderColour = Color4.Gray,
                            BorderThickness = 2,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.Centre,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                AlwaysPresent = true,
                                Colour = Color4.White,
                            }
                        },
                        new CircularContainer
                        {
                            Position = new Vector2(0, 40),
                            Size = new Vector2(20),
                            Masking = true,
                            BorderColour = Color4.Gray,
                            BorderThickness = 2,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                AlwaysPresent = true,
                                Colour = Color4.White,
                            }
                        }
                    }
                },
                flash = new FlashPiece(),
                explode = new ExplodePiece()
            };
        }

        private readonly IBindable<ArmedState> state = new Bindable<ArmedState>();
        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            Hold osuObject = (Hold)drawableObject.HitObject;

            state.BindTo(drawableObject.State);
            state.BindValueChanged(updateState, true);

            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour =>
            {
                explode.Colour = colour.NewValue;
                note.Colour = colour.NewValue;
                Glow.Colour = colour.NewValue;
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
                            note.FadeOut();
                            this.FadeOut(800);
                        }
                    }
                    break;
            }
        }
    }
}
