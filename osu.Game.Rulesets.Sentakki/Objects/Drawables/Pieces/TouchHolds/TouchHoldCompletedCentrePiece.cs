using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds
{
    public partial class TouchHoldCompletedCentre : CompositeDrawable
    {
        private CircularProgress[] progressParts;

        [Resolved]
        private Bindable<Color4[]> paletteBindable { get; set; } = null!;

        public TouchHoldCompletedCentre()
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Size = new Vector2(80);
            Masking = true;
            CornerRadius = 20;
            Rotation = 45;
            Alpha = 0;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black,
                Radius = 10f,
            };
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(2),
                    Rotation = -45f,
                    Children = progressParts =
                    [
                        new CircularProgress
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Progress = 1,
                        },
                        new CircularProgress
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Progress = 0.75 ,
                        },
                        new CircularProgress
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Progress = 0.5 ,
                        },
                        new CircularProgress
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            InnerRadius = 1,
                            Size = Vector2.One,
                            RelativeSizeAxes = Axes.Both,
                            Progress = 0.25 ,
                        },
                    ]
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            paletteBindable.BindValueChanged(p =>
            {
                for (int i = 0; i < progressParts.Length; ++i)
                    progressParts[i].Colour = p.NewValue[^(i + 1)];
            }, true);
        }
    }
}
