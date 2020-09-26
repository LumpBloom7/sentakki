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
using System;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class TouchHoldProgressPiece : CompositeDrawable
    {
        private CircularProgress redProgress;
        private CircularProgress yellowProgress;
        private CircularProgress greenProgress;
        private CircularProgress blueProgress;
        private OsuColour colours = new OsuColour();

        public BindableDouble ProgressBindable = new BindableDouble();

        public TouchHoldProgressPiece()
        {
            ProgressBindable.BindValueChanged(p =>
            {
                redProgress.Current.Value = Math.Min(p.NewValue, .25);
                yellowProgress.Current.Value = Math.Min(p.NewValue, .50);
                greenProgress.Current.Value = Math.Min(p.NewValue, .75);
                blueProgress.Current.Value = p.NewValue;
            });
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Masking = true;
            BorderColour = Color4.White;
            BorderThickness = 3;
            Alpha = .8f;
            Size = new Vector2(110);
            CornerRadius = 2.5f;
            CornerRadius = 27.5f;
            Rotation = 45;
            InternalChildren = new Drawable[]{
                new Container {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(2),
                    Rotation = -45f,
                    Children = new Drawable[]{
                        blueProgress = new CircularProgress{
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Current = { Value = 0 },
                            Colour = colours.Blue
                        },
                        greenProgress = new CircularProgress{
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Current = { Value = 0 },
                            Colour = colours.Green
                        },
                        yellowProgress = new CircularProgress{
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Current = { Value = 0 },
                            Colour = colours.Yellow,
                        },
                        redProgress = new CircularProgress{
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Current = { Value = 0 },
                            Colour = colours.Red,
                        },
                    }
                }
            };
        }

    }
}
