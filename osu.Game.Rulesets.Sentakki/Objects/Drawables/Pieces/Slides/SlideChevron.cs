using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides
{
    public partial class SlideChevron : PoolableDrawable, ISlideChevron
    {
        public double DisappearThreshold { get; set; }
        public SlideVisual? SlideVisual { get; set; }
        private DrawableChevron chevron = null!;

        public float Thickness
        {
            get => chevron.Thickness;
            set => chevron.Thickness = value;
        }

        public SlideChevron()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(80, 60);

        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(chevron = new DrawableChevron()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,

                Thickness = 6.5f
            });
        }
    }
}
