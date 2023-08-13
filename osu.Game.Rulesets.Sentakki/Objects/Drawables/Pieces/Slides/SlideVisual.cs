using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Configuration;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides
{
    public partial class SlideVisual : CompositeDrawable
    {
        // This will be proxied, so a must.
        public override bool RemoveWhenNotAlive => false;

        public double Progress { get; set; }

        private SentakkiSlidePath path = null!;

        public SentakkiSlidePath Path
        {
            get => path;
            set
            {
                path = value;
                Progress = 0;
                updateVisuals();
                UpdateChevronVisibility();
            }
        }

        public void UpdateChevronVisibility()
        {
            for (int i = 0; i < chevrons.Count; i++)
                ISlideChevron.UpdateProgress((ISlideChevron)chevrons[i]);
        }

        public SlideVisual()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [Resolved]
        private DrawablePool<SlideChevron>? chevronPool { get; set; }

        private Container<Drawable> chevrons = null!;

        private readonly BindableBool snakingIn = new BindableBool(true);

        private readonly List<SlideFanChevron> fanChevrons = new List<SlideFanChevron>();

        [BackgroundDependencyLoader]
        private void load(SentakkiRulesetConfigManager? sentakkiConfig, SlideFanChevrons? fanChevrons)
        {
            sentakkiConfig?.BindWith(SentakkiRulesetSettings.SnakingSlideBody, snakingIn);

            AddRangeInternal(new Drawable[]
            {
                chevrons = new Container()
            });

            if (fanChevrons != null)
            {
                for (int i = 0; i < 11; ++i)
                    this.fanChevrons.Add(new SlideFanChevron(fanChevrons.Get(i)));
            }
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
        private const double chevrons_per_distance = chevrons_per_eith * 8 / (2 * Math.PI * ring_radius);
        private const double endpoint_distance = 30; // margin for each end

        private static int chevronsInContinuousPath(SliderPath path)
        {
            return (int)Math.Ceiling((path.Distance - (2 * endpoint_distance)) * chevrons_per_distance);
        }

        private void tryCreateRegularChevrons()
        {
            if (chevronPool is null)
                return;

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
                    chevron.DisappearThreshold = (runningDistance + distance) / this.path.TotalDistance;
                    chevron.Rotation = angle;
                    chevron.Depth = chevrons.Count;
                    chevron.SlideVisual = this;
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

            var delta = path.PositionAt(1) - path.FanOrigin;

            for (int i = 0; i < 11; ++i)
            {
                float progress = (i + 1) / (float)12;
                float scale = progress;
                SlideFanChevron fanChev = fanChevrons[i];

                const float safe_space_ratio = 570 / 600f;

                float y = safe_space_ratio * scale;

                fanChev.Position = path.FanOrigin + (delta * y);
                fanChev.Rotation = fanChev.Position.GetDegreesFromPosition(path.FanOrigin);

                fanChev.DisappearThreshold = path.FanStartProgress + ((i + 1) / 11f * (1 - path.FanStartProgress));
                fanChev.Depth = chevrons.Count;
                fanChev.SlideVisual = this;

                chevrons.Add(fanChev);
            }
        }

        public void PerformEntryAnimation(double duration)
        {
            if (snakingIn.Value)
            {
                double fadeDuration = duration / chevrons.Count;
                double currentOffset = duration / 2;
                double offsetIncrement = (duration - currentOffset - fadeDuration) / (chevrons.Count - 1);

                for (int j = chevrons.Count - 1; j >= 0; j--)
                {
                    var chevron = chevrons[j];
                    chevron.FadeOut()
                           .Delay(currentOffset)
                           .FadeIn(fadeDuration)
                           // This finally clause ensures the chevron maintains the correct visibility state after a rewind
                           .Finally(static c => ISlideChevron.UpdateProgress((ISlideChevron)c));

                    currentOffset += offsetIncrement;
                }
            }
            else
            {
                chevrons.FadeOut().Delay(duration / 2).FadeIn(duration / 2);
            }
        }

        public void PerformExitAnimation(double duration)
        {
            bool found = false;
            double fadeDuration = 0;
            double currentOffset = 0;

            for (int i = 0; i < chevrons.Count; ++i)
            {
                var chevron = chevrons[i];

                if (((ISlideChevron)chevron).DisappearThreshold <= Progress)
                {
                    chevron.FadeOut();
                    continue;
                }

                if (!found)
                {
                    found = true;
                    fadeDuration = duration / (chevrons.Count - i);
                    currentOffset = (fadeDuration / 2) * (chevrons.Count - i);
                }

                chevron.FadeIn().Delay(currentOffset).FadeOut(fadeDuration);
                currentOffset -= fadeDuration / 2;
            }
        }

        public void Free() => chevrons.Clear(false);
    }
}
