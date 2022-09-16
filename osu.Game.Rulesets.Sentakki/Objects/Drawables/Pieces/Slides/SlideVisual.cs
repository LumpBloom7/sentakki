using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides
{
    public class SlideVisual : SlideVisualBase<CompositeDrawable>
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

        private List<SlideFanChevron> fanChevrons = new List<SlideFanChevron>();

        private const int chevrons_per_eith = 8;
        private const double ring_radius = 297;
        private const double chevrons_per_distance = (chevrons_per_eith * 8) / (2 * Math.PI * ring_radius);
        private const double endpoint_distance = 30; // margin for each end

        private static int chevronsInContinuousPath(SliderPath path)
        {
            return (int)Math.Ceiling((path.Distance - (2 * endpoint_distance)) * chevrons_per_distance);
        }

        [BackgroundDependencyLoader]
        private void load(SlideFanChevrons fanChevrons)
        {
            for (int i = 0; i < 11; ++i)
                this.fanChevrons.Add(new SlideFanChevron(fanChevrons.Get(i)));
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


            if (path.HasFanSlide)
            {
                var origin = (path.SlideSegments.Length > 0) ? path.SlideSegments[^1].PositionAt(1) : path.PositionAt(0);
                var delta = path.PositionAt(1) - origin;
                float length = delta.Length;
                var direction = delta.Normalized();

                double fanDistDelta = path.TotalDistance - path.FanStartDistance;
                double fanDistDeltaRatio = fanDistDelta / path.TotalDistance;
                double fanDistStartRatio = path.FanStartDistance / path.TotalDistance;

                for (int i = 0; i < 11; ++i)
                {
                    float progress = (i + 1) / (float)12;
                    float scale = progress;
                    SlideFanChevron fanChev = fanChevrons[i];

                    var safeSpaceRatio = 570 / 600f;
                    var endPointRatio = 0 / 600f;

                    var Y = safeSpaceRatio * scale + endPointRatio;


                    fanChev.Position = origin + (delta * Y);
                    fanChev.Rotation = fanChev.Position.GetDegreesFromPosition(origin);

                    fanChev.Progress = fanDistStartRatio + ((i + 1) / (float)11) * fanDistDeltaRatio;
                    fanChev.Depth = Chevrons.Count;

                    Chevrons.Add(fanChev);
                };
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
                AddInternal(new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = textures.Get("slide"),
                });
            }

            protected override void FreeAfterUse()
            {
                base.FreeAfterUse();
                ClearTransforms();
            }
        }

        public class SlideFanChevron : CompositeDrawable, ISlideChevron
        {
            public double Progress { get; set; }
            private readonly IBindable<Vector2> textureSize = new Bindable<Vector2>();

            public SlideFanChevron((BufferedContainerView<Drawable> view, IBindable<Vector2> sizeBindable) chevron)
            {
                Anchor = Origin = Anchor.Centre;

                textureSize.BindValueChanged(v => Size = v.NewValue);
                textureSize.BindTo(chevron.sizeBindable);

                AddInternal(chevron.view);
            }
        }
    }
}
