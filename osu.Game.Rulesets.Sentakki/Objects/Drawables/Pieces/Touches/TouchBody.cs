using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Touches
{
    public partial class TouchBody : Container
    {
        public Container BorderContainer;
        public Container PieceContainer;

        public TouchBody()
        {
            Size = new Vector2(130);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Alpha = 0;

            InternalChildren = new Drawable[]
            {
                PieceContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]{
                                new TouchGlowPiece
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                },
                                new TouchGlowPiece
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.TopCentre,
                                    Rotation = 180
                                },
                                new TouchGlowPiece
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.TopCentre,
                                    Rotation = 270
                                },
                                new TouchGlowPiece
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.TopCentre,
                                    Rotation = 90
                                },
                            }
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]{
                                new TouchPiece
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                },
                                new TouchPiece
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.TopCentre,
                                    Rotation = 180
                                },
                                new TouchPiece
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.TopCentre,
                                    Rotation = 270
                                },
                                new TouchPiece
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.TopCentre,
                                    Rotation = 90
                                },
                            }
                        },

                        new DotPiece()
                    }
                },
                BorderContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(100),
                    CornerRadius = 25f,
                    CornerExponent = 2.5f,
                    Masking = true,
                    BorderThickness = 10,
                    BorderColour = Color4.White,
                    Alpha = 0,
                    Child = new Box
                    {
                        Alpha = 0,
                        AlwaysPresent = true,
                        RelativeSizeAxes = Axes.Both
                    }
                },
            };
        }

        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour =>
            {
                PieceContainer.Colour = colour.NewValue;
            }, true);
        }
    }
}
