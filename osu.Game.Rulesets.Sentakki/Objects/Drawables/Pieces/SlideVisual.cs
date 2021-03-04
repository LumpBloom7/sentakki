using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Configuration;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
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
        private SliderPath path;

        public SliderPath Path
        {
            get => path;
            set
            {
                if (path == value)
                    return;
                path = value;
                updateVisuals();
                updateProgress();
            }
        }

        private Container<SlideSegment> segments;
        private DrawablePool<SlideSegment> segmentPool;
        private DrawablePool<SlideChevron> chevronPool;

        private readonly BindableBool snakingIn = new BindableBool(true);

        public int SegmentCount => segments.Count;

        public SlideVisual()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfig)
        {
            sentakkiConfig?.BindWith(SentakkiRulesetSettings.SnakingSlideBody, snakingIn);

            AddRangeInternal(new Drawable[]{
                segmentPool = new DrawablePool<SlideSegment>(5),
                chevronPool = new DrawablePool<SlideChevron>(61),
                segments = new Container<SlideSegment>(),
            });
        }

        private const int chevrons_per_eith = 8;
        private const double ring_radius = 297;
        private const double chevrons_per_distance = (chevrons_per_eith * 8) / (2 * Math.PI * ring_radius);
        private const double distance_per_chevron = 1 / chevrons_per_distance;
        private const double endpoint_distance = /*r*/34;

        /// <returns>A rented array which needs to be returned to <see cref="ArrayPool&lt;SliderPath&gt;.Shared"/></returns>
        private static (SliderPath[] autosmoothedarray, SliderPath[] originalarray, int count) splitIntoContinuousPaths(SliderPath path)
        {
            var paths = ArrayPool<SliderPath>.Shared.Rent(path.ControlPoints.Skip(1).SkipLast(1).Count(c => c.Type.Value == null || c.Type.Value == PathType.Linear));
            var originalPaths = ArrayPool<SliderPath>.Shared.Rent(path.ControlPoints.Skip(1).SkipLast(1).Count(c => c.Type.Value == null || c.Type.Value == PathType.Linear));
            const double auto_smooth_angle = 60;

            PathType context = path.ControlPoints[0].Type.Value ?? PathType.Linear;
            int pathIndex = 0;

            if (paths[0] is null) paths[0] = new SliderPath();
            paths[0].ControlPoints.Clear();
            paths[0].ControlPoints.Add(path.ControlPoints[0]);
            if (originalPaths[0] is null) originalPaths[0] = new SliderPath();
            originalPaths[0].ControlPoints.Clear();
            originalPaths[0].ControlPoints.Add(path.ControlPoints[0]);

            for (int i = 1; i < path.ControlPoints.Count; i++)
            {
                var controlPoint = path.ControlPoints[i];
                paths[pathIndex].ControlPoints.Add(controlPoint);
                originalPaths[pathIndex].ControlPoints.Add(controlPoint);
                context = controlPoint.Type.Value ?? context;
                if (context == PathType.Linear && i + 1 != path.ControlPoints.Count)
                {
                    var previousPosition = path.ControlPoints[i - 1].Position.Value;
                    var nextPosition = path.ControlPoints[i + 1].Position.Value;
                    var prev = controlPoint.Position.Value.GetDegreesFromPosition(previousPosition);
                    var next = controlPoint.Position.Value.GetDegreesFromPosition(nextPosition);
                    if (SentakkiExtensions.GetDeltaAngle(prev, next) >= 180 - auto_smooth_angle)
                    {
                        // prev -> +bezier -> current(null) -> +bezier -> next
                        paths[pathIndex].ControlPoints.RemoveAt(paths[pathIndex].ControlPoints.Count - 1);
                        paths[pathIndex].ControlPoints.Add(new PathControlPoint((previousPosition + (controlPoint.Position.Value * 8)) / 9, PathType.Bezier));
                        paths[pathIndex].ControlPoints.Add(new PathControlPoint(controlPoint.Position.Value));
                        paths[pathIndex].ControlPoints.Add(new PathControlPoint((nextPosition + (controlPoint.Position.Value * 8)) / 9, PathType.Bezier));
                        continue;
                    }

                    pathIndex++;
                    if (paths[pathIndex] == null) paths[pathIndex] = new SliderPath();
                    paths[pathIndex].ControlPoints.Clear();
                    paths[pathIndex].ControlPoints.Add(controlPoint);
                    if (originalPaths[pathIndex] == null) originalPaths[pathIndex] = new SliderPath();
                    originalPaths[pathIndex].ControlPoints.Clear();
                    originalPaths[pathIndex].ControlPoints.Add(controlPoint);
                }
            }
            return (paths, originalPaths, pathIndex + 1);
        }

        private static int chevronsInContinuousPath(SliderPath path)
        {
            return (int)Math.Ceiling((path.Distance - (2 * endpoint_distance)) * chevrons_per_distance);
        }

        private void updateVisuals()
        {
            foreach (var segment in segments)
                segment.ClearChevrons();
            segments.Clear(false);

            SlideSegment createSegment(SliderPath path)
            {
                SlideSegment currentSegment = segmentPool.Get();
                var chevronCount = chevronsInContinuousPath(path);
                var totalDistance = path.Distance;
                var safeDistance = totalDistance - (endpoint_distance * 2);

                var previousPosition = path.PositionAt(0);
                for (int i = 0; i < chevronCount; i++)
                {
                    var progress = (double)i / (chevronCount - 1); // from 0 to 1, both inclusive
                    var position = path.PositionAt(((progress * safeDistance) + endpoint_distance) / totalDistance);
                    var angle = previousPosition.GetDegreesFromPosition(position);

                    var chevron = chevronPool.Get();
                    chevron.Position = position;
                    chevron.Rotation = angle;
                    chevron.Depth = i;
                    currentSegment.Add(chevron);

                    previousPosition = position;
                }
                return currentSegment;
            }

            var (paths, originalPaths, count) = splitIntoContinuousPaths(path);
            double totalDistance = 0;
            for ( int i = 0; i < count; i++ )
            {
                var segment = createSegment(paths[i]);
                segment.StartProgress = (totalDistance + endpoint_distance) / path.Distance;
                segment.EndProgress = (totalDistance + originalPaths[i].Distance - endpoint_distance) / path.Distance;
                segment.Depth = segments.Count;
                totalDistance += originalPaths[i].Distance;
                segments.Add(segment);
            }
        }

        private void updateProgress()
        {
            for (int i = 0; i < segments.Count; i++)
            {
                segments[i].UpdateProgress(progress);
            }
        }

        public void PerformEntryAnimation(double duration)
        {
            updateProgress(); // transforms are reset so we need to reapply them
            if (snakingIn.Value)
            {
                double fadeDuration = duration / segments.Sum(s => s.ChevronCount);
                double currentOffset = duration / 2;
                for (int i = segments.Count - 1; i >= 0; i--)
                {
                    var segment = segments[i];
                    for (int j = segment.Children.Count - 1; j >= 0; j--)
                    {
                        var chevron = segment.Children[j] as SlideChevron;
                        chevron.FadeOut().Delay(currentOffset).FadeInFromZero(fadeDuration * 2);
                        currentOffset += fadeDuration / 2;
                    }
                }
            }
            else
            {
                this.FadeOut().Delay(duration / 2).FadeIn(duration / 2);
            }
        }

        public void PerformExitAnimation(double duration)
        {
            updateProgress();
            int chevronsLeft = segments.Sum(s => s.ChevronCount);
            double fadeDuration() => duration / chevronsLeft;
            double currentOffset = 0;
            for (int i = segments.Count - 1; i >= 0; i--)
            {
                var segment = segments[i];
                for (int j = segment.Children.Count - 1; j >= 0; j--)
                {
                    var chevron = segment.Children[j] as SlideChevron;
                    chevron.Delay(currentOffset).FadeOut(fadeDuration() * 2);
                    currentOffset += fadeDuration() / 2;
                    chevronsLeft--;
                }
            }
        }

        private class SlideSegment : PoolableDrawable
        {
            public void ClearChevrons() => ClearInternal(false);
            public void Add(Drawable drawable) => AddInternal(drawable);
            public int ChevronCount => InternalChildren.Count;

            public IReadOnlyList<Drawable> Children => InternalChildren;
            public double StartProgress;
            public double EndProgress;
            public void UpdateProgress(double progress)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    var progressHere = (double)i / (Children.Count - 1);
                    progressHere = StartProgress + (progressHere * (EndProgress - StartProgress));

                    if (progressHere > progress)
                        Children[^(i+1)].FadeIn();
                    else
                        Children[^(i+1)].FadeOut();
                }

                if (progress >= EndProgress)
                    this.FadeOut();
                else
                    this.FadeIn();
            }
        }

        private class SlideChevron : PoolableDrawable
        {
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
        }
    }
}
