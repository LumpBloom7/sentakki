using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Configuration;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides
{
    public class SlideVisual : CompositeDrawable
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

        private void updateProgress()
        {
            for (int i = 0; i < chevrons.Count; i++)
                ISlideChevron.UpdateProgress((ISlideChevron)chevrons[i], progress);
        }

        public SlideVisual()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [Resolved(canBeNull: true)]
        private DrawablePool<SlideChevron> chevronPool { get; set; }

        private Container chevrons;

        private readonly BindableBool snakingIn = new BindableBool(true);

        private List<SlideFanChevron> fanChevrons = new List<SlideFanChevron>();

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfig, SlideFanChevrons fanChevrons)
        {
            sentakkiConfig?.BindWith(SentakkiRulesetSettings.SnakingSlideBody, snakingIn);

            AddRangeInternal(new Drawable[]{
                chevrons = new Container()
            });

            if (fanChevrons != null)
                for (int i = 0; i < 11; ++i)
                    this.fanChevrons.Add(new SlideFanChevron(fanChevrons.Get(i)));
        }

        private void updateVisuals()
        {
            chevrons.Clear(false);

            // Create regular slide chevrons if needed
            tryCreateRegularChevrons();

            // Create fan slide chevrons if needed
            tryCreateFanChevrons();
        }

        private const int chevrons_per_eith = 8;
        private const double ring_radius = 297;
        private const double chevrons_per_distance = (chevrons_per_eith * 8) / (2 * Math.PI * ring_radius);
        private const double endpoint_distance = 30; // margin for each end

        private static int chevronsInContinuousPath(SliderPath path)
        {
            return (int)Math.Ceiling((path.Distance - (2 * endpoint_distance)) * chevrons_per_distance);
        }

        private void tryCreateRegularChevrons()
        {
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
                    chevron.Depth = chevrons.Count;
                    chevrons.Add(chevron);

                    previousPosition = position;
                }
                runningDistance += totalDistance;
            }
        }

        private void tryCreateFanChevrons()
        {
            if (!path.EndsWithSlideFan)
                return;

            var delta = path.PositionAt(1) - path.fanOrigin;

            for (int i = 0; i < 11; ++i)
            {
                float progress = (i + 1) / (float)12;
                float scale = progress;
                SlideFanChevron fanChev = fanChevrons[i];

                float safeSpaceRatio = 570 / 600f;

                float Y = safeSpaceRatio * scale;

                fanChev.Position = path.fanOrigin + (delta * Y);
                fanChev.Rotation = fanChev.Position.GetDegreesFromPosition(path.fanOrigin);

                fanChev.Progress = path.FanStartProgress + ((i + 1) / 11f * (1 - path.FanStartProgress));
                fanChev.Depth = chevrons.Count;

                chevrons.Add(fanChev);
            };
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

            void finalSteps(Drawable chevron) => ISlideChevron.UpdateProgress((ISlideChevron)chevron, progress);
        }

        public void PerformExitAnimation(double duration)
        {
            int chevronsLeft = chevrons.Children.Count(c => c.Alpha != 0);
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

        public void Free() => chevrons.Clear(false);
    }
}
