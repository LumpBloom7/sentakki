using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds
{
    public partial class TouchHoldProgressPiece : CompositeDrawable
    {
        public BindableDouble ProgressBindable = new BindableDouble();

        private CircularProgress[] progressParts;

        [Resolved]
        private Bindable<Color4[]> paletteBindable { get; set; } = null!;

        public TouchHoldProgressPiece()
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Masking = true;
            BorderColour = Color4.White;
            BorderThickness = 3;
            Alpha = .8f;
            Size = new Vector2(110);
            CornerRadius = 27.5f;
            Rotation = 45;
            InternalChildren =
            [
                new Container
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(2),
                    Rotation = -45f,
                    Children = progressParts = [
                        new CircularProgress
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Progress = 0,
                        },
                        new CircularProgress
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Progress = 0,
                        },
                        new CircularProgress
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Progress = 0,
                        },
                        new CircularProgress
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Progress = 0,
                        }
                    ]
                }
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ProgressBindable.BindValueChanged(p =>
            {
                for (int i = 0; i < progressParts.Length; ++i)
                    progressParts[^(i + 1)].Progress = Math.Min(p.NewValue, 0.25 * (i + 1));
            }, true);

            paletteBindable.BindValueChanged(p =>
            {
                for (int i = 0; i < progressParts.Length; ++i)
                    progressParts[i].Colour = p.NewValue[^(i + 1)];
            }, true);
        }
    }
}
