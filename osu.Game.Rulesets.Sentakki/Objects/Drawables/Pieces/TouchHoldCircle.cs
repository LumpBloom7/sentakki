// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class TouchHoldCircle : CircularContainer
    {
        public double Duration;

        public readonly GlowPiece Glow;
        private readonly ExplodePiece explode;
        private readonly FlashPiece flash;
        private readonly CircularProgress progress;
        private readonly Sprite disc;
        private readonly CircularContainer fillCircle;
        private readonly CircularContainer ring;
        private readonly CircularContainer outline;
        private readonly SpriteText text;

        public TouchHoldCircle()
        {
            RelativeSizeAxes = Axes.Both;
            Size = Vector2.One;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                Glow = new GlowPiece(){
                    Alpha = 0f,
                },
                ring = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Padding = new MarginPadding(1),
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                        },
                        new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = Vector2.One,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            Child = progress = new CircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                InnerRadius = 0.250f,
                                Size = Vector2.One,
                                RelativeSizeAxes = Axes.Both,
                                Current = { Value = 0 },
                            }
                        },
                    }
                },
                outline = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderColour = Color4.Black,
                    BorderThickness = 2,
                    Alpha = .5f,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true,
                    }
                },
                fillCircle = new CircularContainer{
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(.77f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    Children = new Drawable[]{
                        disc = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        new TrianglesPiece
                        {
                            RelativeSizeAxes = Axes.Both,
                            Blending = BlendingParameters.Additive,
                            Alpha = 0.5f,
                        }
                    }
                },
                text = new SpriteText
                {
                    Text = "HOLD!",
                    Font = OsuFont.Torus.With(weight: FontWeight.Bold,size:32),
                    Colour = Color4.White,
                    Shadow = true,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                flash = new FlashPiece(),
                explode = new ExplodePiece(),
            };
        }

        public void StartProgressBar()
        {
            progress.Delay(500).FillTo(1f, Duration);
        }

        private readonly IBindable<ArmedState> state = new Bindable<ArmedState>();
        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, DrawableHitObject drawableObject)
        {
            disc.Texture = textures.Get(@"Gameplay/osu/disc");

            state.BindTo(drawableObject.State);
            state.BindValueChanged(updateState, true);

            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour =>
            {
                explode.Colour = colour.NewValue;
                Glow.Colour = colour.NewValue;
                progress.Colour = colour.NewValue;
                fillCircle.Colour = colour.NewValue;
            }, true);
        }

        private void updateState(ValueChangedEvent<ArmedState> state)
        {
            Glow.FadeOut(400);

            switch (state.NewValue)
            {
                case ArmedState.Hit:
                    const double flash_in = 40;
                    const double flash_out = 100;

                    flash.Delay(Duration).FadeTo(0.8f, flash_in)
                         .Then()
                         .FadeOut(flash_out);

                    explode.Delay(Duration).FadeIn(flash_in);
                    this.Delay(Duration).ScaleTo(1.5f, 400, Easing.OutQuad);

                    using (BeginDelayedSequence(Duration + flash_in, true))
                    {
                        //after the flash, we can hide some elements that were behind it
                        progress.FadeOut();
                        fillCircle.FadeOut();
                        outline.FadeOut();
                        ring.FadeOut();
                        text.FadeOut();

                        this.FadeOut(800);
                    }
                    break;
            }
        }
    }
}
