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

        private Container<SlideChevron> chevrons;
        private DrawablePool<SlideChevron> chevronPool;

        private readonly BindableBool snakingIn = new BindableBool(true);

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
                chevronPool = new DrawablePool<SlideChevron>(61),
                chevrons = new Container<SlideChevron>(),
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
            var paths = ArrayPool<SliderPath>.Shared.Rent(path.ControlPoints.Skip(1).SkipLast(1).Count(c => c.Type.Value == null || c.Type.Value == PathType.Linear));

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

        private void updateVisuals()
        {
            chevrons.Clear(false);

            double runningDistance = 0;
            void createSegment(SliderPath path)
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
                    chevron.Progress = (runningDistance + distance) / this.path.Distance;
                    chevron.Rotation = angle;
                    chevrons.Alpha = 1;
                    chevron.Depth = chevrons.Count;
                    chevrons.Add(chevron);

                    previousPosition = position;
                }
                runningDistance += totalDistance;
            }

            var (paths, count) = splitIntoContinuousPaths(path);
            for ( int i = 0; i < count; i++ )
            {
                createSegment(paths[i]);
            }

            ArrayPool<SliderPath>.Shared.Return(paths);
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
            updateProgress(); // transforms are reset so we need to reapply them
            if (snakingIn.Value)
            {
                double fadeDuration = duration / chevrons.Count;
                double currentOffset = duration / 2;
                for (int j = chevrons.Count - 1; j >= 0; j--)
                {
                    var chevron = chevrons[j];
                    chevron.FadeOut().Delay(currentOffset).FadeInFromZero(fadeDuration * 2);
                    currentOffset += fadeDuration / 2;
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

        private class SlideChevron : PoolableDrawable
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

            public void UpdateProgress(double progress)
            {
                Alpha = progress >= Progress ? 0 : 1;
            }
        }
    }
}
