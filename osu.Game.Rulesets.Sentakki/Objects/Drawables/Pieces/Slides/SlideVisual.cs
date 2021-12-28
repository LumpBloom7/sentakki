using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Configuration;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides
{
    public class SlideVisual : CompositeDrawable, ISlideVisual
    {
        // This will be proxied, so a must.
        public override bool RemoveWhenNotAlive => false;

        private double progress;
        public double Progress
        {
            get => progress;
            set
            {
                progress = value;
                updateProgress();
            }
        }
        private SentakkiSlidePath path;

        public SentakkiSlidePath Path
        {
            get => path;
            set
            {
                path = value;
                updateVisuals();
                updateProgress();
            }
        }

        private Container<SlideChevron> chevrons;

        private readonly BindableBool snakingIn = new BindableBool(true);

        public SlideVisual()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [Resolved]
        private DrawablePool<SlideChevron> chevronPool { get; set; }

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfig)
        {
            sentakkiConfig?.BindWith(SentakkiRulesetSettings.SnakingSlideBody, snakingIn);

            AddRangeInternal(new Drawable[]{
                chevrons = new Container<SlideChevron>(),
            });
        }

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
            chevrons.Clear(false);

            double runningDistance = 0;
            foreach (var path in path.SlideSegments)
            {
                var chevronCount = chevronsInContinuousPath(path);
                var totalDistance = path.Distance;
                var safeDistance = totalDistance - (endpoint_distance * 2);

                var previousPosition = path.PositionAt(0);
                for (int i = 0; i < chevronCount; i++)
                {
                    var progress = (double)i / (chevronCount - 1); // from 0 to 1, both inclusive
                    var distance = (progress * safeDistance) + endpoint_distance;
                    progress = distance / totalDistance;
                    var position = path.PositionAt(progress);
                    var angle = previousPosition.GetDegreesFromPosition(position);

                    var chevron = chevronPool.Get();
                    chevron.Position = position;
                    chevron.Progress = (runningDistance + distance) / this.path.TotalDistance;
                    chevron.Rotation = angle;
                    chevron.Depth = chevrons.Count;
                    chevrons.Add(chevron);

                    previousPosition = position;
                }
                runningDistance += totalDistance;
            }
        }

        private void updateProgress()
        {
            for (int i = 0; i < chevrons.Count; i++)
            {
                chevrons[i].UpdateProgress(progress);
            }
        }

        public void PerformEntryAnimation(double duration)
        {
            if (snakingIn.Value)
            {
                double fadeDuration = duration / chevrons.Count;
                double currentOffset = duration / 2;
                for (int j = chevrons.Count - 1; j >= 0; j--)
                {
                    var chevron = chevrons[j];
                    chevron.FadeOut().Delay(currentOffset).FadeIn(fadeDuration * 2).Finally(finalSteps);
                    currentOffset += fadeDuration / 2;
                }
            }
            else
            {
                chevrons.FadeOut().Delay(duration / 2).FadeIn(duration / 2);
            }

            void finalSteps(SlideChevron chevron) => chevron.UpdateProgress(progress);
        }

        public void PerformExitAnimation(double duration)
        {
            int chevronsLeft = chevrons.Count(c => c.Alpha != 0);
            double fadeDuration = duration / chevronsLeft;
            double currentOffset = 0;
            for (int j = chevrons.Count - 1; j >= 0; j--)
            {
                var chevron = chevrons[j];
                if (chevron.Alpha == 0)
                {
                    continue;
                }
                chevron.Delay(currentOffset).FadeOut(fadeDuration * 2);
                currentOffset += fadeDuration / 2;
            }
        }

        public void Free()
        {
            chevrons.Clear(false);
        }

        public class SlideChevron : PoolableDrawable
        {
            public double Progress;

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

            public void UpdateProgress(double progress)
            {
                Alpha = progress >= Progress ? 0 : 1;
            }
        }
    }
}
