using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

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

        public void UpdateProgress(SlideChevron chevron)
        {
            chevron.Alpha = Progress >= chevron.DisappearThreshold ? 0 : 1;
        }

        public void UpdateChevronVisibility()
        {
            for (int i = 0; i < chevrons.Count; i++)
                UpdateProgress(chevrons[i]);
        }

        public SlideVisual()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [Resolved]
        private DrawablePool<SlideChevron>? chevronPool { get; set; }

        private Container<SlideChevron> chevrons = null!;

        private readonly BindableBool snakingIn = new BindableBool(true);

        [BackgroundDependencyLoader]
        private void load(SentakkiRulesetConfigManager? sentakkiConfig)
        {
            sentakkiConfig?.BindWith(SentakkiRulesetSettings.SnakingSlideBody, snakingIn);

            AddRangeInternal(new Drawable[]
            {
                chevrons = new Container<SlideChevron>()
            });
        }

        private void updateVisuals()
        {
            chevrons.Clear(false);

            // Create regular slide chevrons if needed
            tryCreateRegularChevrons();

            // Create fan slide chevrons if needed
            tryCreateFanChevrons();
        }

        private const int chevrons_per_eith = 9;
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

                    chevron.Thickness = 6.5f;
                    chevron.Height = 60;
                    chevron.FanChevron = false;
                    chevron.Width = 80;
                    chevrons.Add(chevron);

                    previousPosition = position;
                }

                runningDistance += totalDistance;
            }
        }

        private void tryCreateFanChevrons()
        {
            if (chevronPool is null)
                return;

            if (!path.EndsWithSlideFan)
                return;

            var delta = path.PositionAt(1) - path.FanOrigin;
            Vector2 lineStart = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0);
            Vector2 middleLineEnd = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 4);
            Vector2 middleLineDelta = middleLineEnd - lineStart;

            for (int i = 0; i < 11; ++i)
            {
                float progress = (i + 2f) / 12f;

                float scale = progress - ((1f) / 12f);
                var middlePosition = lineStart + (middleLineDelta * progress);

                float t = 6.5f + (2.5f * scale);

                float chevWidth = MathF.Abs(lineStart.X - middlePosition.X) - t;

                (float sin, float cos) = MathF.SinCos((-135 + 90f) / 180f * MathF.PI);

                Vector2 secondPoint = new Vector2(sin, -cos) * chevWidth;
                Vector2 one = new Vector2(chevWidth, 0);

                var middle = (one + secondPoint) * 0.5f;
                float h = (middle - Vector2.Zero).Length + (t * 3);

                float w = (secondPoint - one).Length;

                const float safe_space_ratio = (570 / 600f);

                float y = safe_space_ratio * scale;

                var chevron = chevronPool.Get();

                chevron.Position = path.FanOrigin + (delta * y);
                chevron.Rotation = chevron.Position.GetDegreesFromPosition(path.PositionAt(1));

                chevron.DisappearThreshold = path.FanStartProgress + ((i + 1) / 11f * (1 - path.FanStartProgress));
                chevron.Depth = chevrons.Count;

                chevron.Width = w + 30;
                chevron.Height = h + 30;
                chevron.Thickness = t;
                chevron.FanChevron = true;

                chevrons.Add(chevron);
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
                           .Finally(c => UpdateProgress(c));

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

                if (chevron.DisappearThreshold <= Progress)
                {
                    chevron.Alpha = 0;
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
