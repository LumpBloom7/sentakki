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
            BorderThickness = 3;
            Alpha = .8f;
            Size = new Vector2(110);
            CornerRadius = 27.5f;
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
                        blueProgress = new TouchHoldCircularProgress(colours.Blue)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Progress = 0,
                            Rotation = 270f
                        },
                        greenProgress = new TouchHoldCircularProgress(colours.Green)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Progress = 0,
                            Rotation = 180
                        },
                        yellowProgress = new TouchHoldCircularProgress(colours.Yellow)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Progress = 0,
                            Rotation = 90
                        },
                        redProgress = new TouchHoldCircularProgress(colours.Red)
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
                redProgress.Progress = Math.Clamp(p.NewValue, 0, .25);
                yellowProgress.Progress = Math.Clamp(p.NewValue - 0.25, 0, .25);
                greenProgress.Progress = Math.Clamp(p.NewValue - 0.5, 0, .25);
                blueProgress.Progress = Math.Clamp(p.NewValue - 0.75, 0, .25);
            });
        }
    }
}
