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

        private int completedSegments;
        public int CompletedSegments
        {
            get => completedSegments;
            set
            {
                completedSegments = value;
                updateProgress(completedSegments);
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
                updateProgress(CompletedSegments);
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
                segmentPool = new DrawablePool<SlideSegment>(20),
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
        private static (SliderPath[] array, int count) splitIntoContinuousPaths(SliderPath path)
        {
            // upperbound, might rent a bit more than needed but it usually returns batches of 16 anyway
            var paths = ArrayPool<SliderPath>.Shared.Rent(path.ControlPoints.Skip(1).SkipLast(1).Count(c => c.Type.Value == null || c.Type.Value == PathType.Linear));
            const double auto_smooth_angle = 60;

            PathType context = path.ControlPoints[0].Type.Value ?? PathType.Linear;
            int pathIndex = 0;

            if (paths[0] is null) paths[0] = new SliderPath();
            paths[0].ControlPoints.Clear();
            paths[0].ControlPoints.Add(path.ControlPoints[0]);

            for (int i = 1; i < path.ControlPoints.Count; i++)
            {
                var controlPoint = path.ControlPoints[i];
                paths[pathIndex].ControlPoints.Add(controlPoint);
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
                }
            }
            return (paths, pathIndex + 1);
        }

        private static int chevronsInContinuousPath(SliderPath path)
        {
            return (int)Math.Ceiling((path.Distance - (2 * endpoint_distance)) * chevrons_per_distance);
        }

        /// <summary>
        /// Creates a list of segments consisting of evenly spaced out points and their directions
        /// </summary>
        public static IEnumerable<(IEnumerable<(Vector2 positon, float rotation)> segment, double startProgress, double endProgress)> CreateSegmentsFor(SliderPath path)
        {
            static IEnumerable<(Vector2 positon, float rotation)> createSegment(SliderPath path)
            {
                var chevronCount = chevronsInContinuousPath(path);
                var totalDistance = path.Distance;
                var safeDistance = totalDistance - (endpoint_distance * 2);

                var previousPosition = path.PositionAt(0);
                for (int i = 0; i < chevronCount; i++)
                {
                    var progress = (double)i / (chevronCount - 1); // from 0 to 1, both inclusive

                    var position = path.PositionAt(((progress * safeDistance) + endpoint_distance) / totalDistance);
                    var angle = previousPosition.GetDegreesFromPosition(position);

                    yield return (position, angle);

                    previousPosition = position;
                }
            }

            var (paths, count) = splitIntoContinuousPaths(path);
            double totalDistance = 0;
            for (int i = 0; i < count; i++)
            {
                yield return (createSegment(paths[i]), (totalDistance + endpoint_distance) / path.Distance, (totalDistance + paths[i].Distance - endpoint_distance) / path.Distance);
                totalDistance += paths[i].Distance;
            }
            ArrayPool<SliderPath>.Shared.Return(paths);
        }

        private int chevronCount;
        private void updateVisuals()
        {
            foreach (var segment in segments)
                segment.ClearChevrons();
            segments.Clear(false);

            chevronCount = 0;
            foreach (var (segment,_,_) in CreateSegmentsFor(path))
            {
                SlideSegment currentSegment = segmentPool.Get();
                currentSegment.Depth = segments.Count;
                segments.Add(currentSegment);

                var left = segment.Count();
                chevronCount += left;
                foreach (var (pos, rot) in segment)
                {
                    currentSegment.Add(chevronPool.Get().With(c => {
                        c.Position = pos;
                        c.Rotation = rot;
                        c.Depth = currentSegment.Children.Count;
                    }));

                    if (currentSegment.Children.Count >= 3 && left >= 3)
                    {
                        currentSegment = segmentPool.Get();
                        currentSegment.Depth = segments.Count;
                        segments.Add(currentSegment);
                    }
                    left--;
                }
            }
        }

        private void updateProgress(int completedNodes)
        {
            for (int i = 1; i <= segments.Count; ++i)
            {
                segments[^i].Alpha = i <= completedNodes ? 0 : 1;
            }
        }

        public void PerformEntryAnimation(double duration)
        {
            if (snakingIn.Value)
            {
                double fadeDuration = duration / chevronCount;
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
            int chevronsLeft = chevronCount;
            double fadeDuration() => duration / chevronsLeft;
            double currentOffset = 0;
            for (int i = segments.Count - 1; i >= 0; i--)
            {
                var segment = segments[i];
                if (segment.Alpha == 0)
                {
                    chevronsLeft -= segment.ChevronCount;
                    continue;
                }

                for (int j = segment.Children.Count - 1; j >= 0; j--)
                {
                    var chevron = segment.Children[j] as SlideChevron;
                    chevron.Delay(currentOffset).FadeOut(fadeDuration() * 2);
                    currentOffset += fadeDuration() / 2;
                }
            }
        }

        private class SlideSegment : PoolableDrawable
        {
            public void ClearChevrons() => ClearInternal(false);
            public void Add(Drawable drawable) => AddInternal(drawable);
            public int ChevronCount => InternalChildren.Count;

            public IReadOnlyList<Drawable> Children => InternalChildren;
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
