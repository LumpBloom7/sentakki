using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class TouchBlob : CircularContainer
    {
        public TouchBlob()
        {
            Size = new Vector2(80);
            Scale = new Vector2(.5f);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Padding = new MarginPadding(1),
                    Child = new Container
                    {
                        Alpha = .5f,
                        Masking = true,
                        RelativeSizeAxes = Axes.Both,
                        CornerRadius = 20,
                        CornerExponent = 2.5f,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Hollow = true,
                            Type = EdgeEffectType.Shadow,
                            Radius = 15,
                            Colour = Color4.Black,
                        }
                    }
                },
                new Container
                {
                    CornerRadius = 20,
                    CornerExponent = 2.5f,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 16.35f,
                    BorderColour = Color4.Gray,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha= 0,
                        AlwaysPresent = true
                    }
                },
                new Container
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(1),
                    Child = new Container
                    {
                        CornerRadius = 20,
                        CornerExponent = 2.5f,
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
                new Container
                {
                    CornerRadius = 20,
                    CornerExponent = 2.5f,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 2,
                    BorderColour = Color4.Gray,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true
                    }
                },
            };
        }

        private readonly IBindable<ArmedState> state = new Bindable<ArmedState>();
        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            state.BindTo(drawableObject.State);
            state.BindValueChanged(updateState, true);
        }

        private void updateState(ValueChangedEvent<ArmedState> state)
        {
            switch (state.NewValue)
            {
                case ArmedState.Hit:
                    const double flash_in = 40;

                    this.Delay(flash_in).FadeOut();

                    break;
            }
        }
    }
}
