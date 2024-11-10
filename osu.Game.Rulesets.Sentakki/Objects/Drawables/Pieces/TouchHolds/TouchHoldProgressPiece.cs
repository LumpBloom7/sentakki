using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds
{
    public partial class TouchHoldProgressPiece : CompositeDrawable
    {
        public BindableDouble ProgressBindable = new BindableDouble();

        public TouchHoldProgressPiece()
        {
            CircularProgress blueProgress;
            CircularProgress greenProgress;
            CircularProgress yellowProgress;
            CircularProgress redProgress;

            OsuColour colours = new OsuColour();
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Masking = true;
            BorderColour = Color4.White;
            BorderThickness = 6;
            Size = new Vector2(130);
            CornerRadius = 30f;
            Rotation = 45;
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(2),
                    Rotation = -45f,
                    Children = new Drawable[]
                    {
                        blueProgress = new CircularProgress
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Progress = 0,
                            Colour = colours.Blue
                        },
                        greenProgress = new CircularProgress
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Progress = 0,
                            Colour = colours.Green
                        },
                        yellowProgress = new CircularProgress
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Progress = 0,
                            Colour = colours.Yellow,
                        },
                        redProgress = new CircularProgress
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Progress = 0,
                            Colour = colours.Red,
                        },
                    }
                }
            };

            ProgressBindable.BindValueChanged(p =>
            {
                redProgress.Progress = Math.Min(p.NewValue, .25);
                yellowProgress.Progress = Math.Min(p.NewValue, .50);
                greenProgress.Progress = Math.Min(p.NewValue, .75);
                blueProgress.Progress = p.NewValue;
            });
        }
    }
}
