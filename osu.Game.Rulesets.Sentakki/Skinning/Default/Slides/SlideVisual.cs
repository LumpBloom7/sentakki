using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default.Slides
{
    public class SlideVisual : SlideVisualBase<SlideVisual.SlideChevron>
    {
        private SentakkiSlidePath path;

        public SentakkiSlidePath Path
        {
            get => path;
            set
            {
                path = value;
                updateVisuals();
                UpdateProgress();
            }
        }

        [Resolved]
        private DrawablePool<SlideChevron> chevronPool { get; set; }

        private const int chevrons_per_eith = 8;
        private const double ring_radius = 297;
        private const double chevrons_per_distance = (chevrons_per_eith * 8) / (2 * Math.PI * ring_radius);
        private const double endpoint_distance = 30; // margin for each end

        private static int chevronsInContinuousPath(SliderPath path)
        {
            return (int)Math.Ceiling((path.Distance - (2 * endpoint_distance)) * chevrons_per_distance);
        }

        private void updateVisuals()
        {
            Chevrons.Clear(false);

            double runningDistance = 0;
            foreach (var path in path.SlideSegments)
            {
                int chevronCount = chevronsInContinuousPath(path);
                double totalDistance = path.Distance;
                double safeDistance = totalDistance - (endpoint_distance * 2);

                var previousPosition = path.PositionAt(0);
                for (int i = 0; i < chevronCount; i++)
                {
                    double progress = (double)i / (chevronCount - 1); // from 0 to 1, both inclusive
                    double distance = (progress * safeDistance) + endpoint_distance;
                    progress = distance / totalDistance;
                    var position = path.PositionAt(progress);
                    float angle = previousPosition.GetDegreesFromPosition(position);

                    var chevron = chevronPool.Get();
                    chevron.Position = position;
                    chevron.Progress = (runningDistance + distance) / this.path.TotalDistance;
                    chevron.Rotation = angle;
                    chevron.Depth = Chevrons.Count;
                    Chevrons.Add(chevron);

                    previousPosition = position;
                }
                runningDistance += totalDistance;
            }
        }

        public override void Free()
        {
            Chevrons.Clear(false);
        }

        public class SlideChevron : PoolableDrawable, ISlideChevron
        {
            public double Progress { get; set; }

            public SlideChevron()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                AddInternal(new SkinnableDrawable(new SentakkiSkinComponent(SentakkiSkinComponents.SlideChevron), _ => new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = textures.Get("slide"),
                }));
            }

            protected override void FreeAfterUse()
            {
                base.FreeAfterUse();
                ClearTransforms();
            }
        }
    }
}
