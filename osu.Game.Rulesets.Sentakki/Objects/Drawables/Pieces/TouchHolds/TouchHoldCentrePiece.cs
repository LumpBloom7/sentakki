using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds
{
    public class TouchHoldCentrePiece : CompositeDrawable
    {
        private OsuColour colours = new OsuColour();
        public TouchHoldCentrePiece()
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Size = new Vector2(80);
            Masking = true;
            CornerRadius = 20f;
            Rotation = 45;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black,
                Radius = 10f,
            };
            InternalChildren = new Drawable[]{
                new Container {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(2),
                    Rotation = -45f,
                    Children = new Drawable[]{
                        new CircularProgress{
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Current = { Value = 1 },
                            Colour = colours.Blue
                        },
                        new CircularProgress{
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Current = { Value = .75 },
                            Colour = colours.Green
                        },
                        new CircularProgress{
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Current = { Value = .5 },
                            Colour = colours.Yellow,
                        },
                        new CircularProgress{
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Current = { Value = .25 },
                            Colour = colours.Red,
                        },
                    }
                },
                new DotPiece()
            };
        }
    }
}
