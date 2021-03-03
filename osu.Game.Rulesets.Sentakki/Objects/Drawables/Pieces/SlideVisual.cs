using System;
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
using osu.Game.Rulesets.Sentakki.Configuration;

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
        private const double chevrons_per_distance = ( chevrons_per_eith * 8 ) / ( 2 * Math.PI * ring_radius );
        private const double distance_per_chevron = 1 / chevrons_per_distance;
        private const double dot_distance = /*r*/34;
        public static int ChevronsInPath (SliderPath path)
        {
            return (int)Math.Ceiling((path.Distance - (2 * dot_distance)) * chevrons_per_distance);
        }

        private void updateVisuals()
        {
            foreach (var segment in segments)
                segment.ClearChevrons();
            segments.Clear(false);

            var chevronCount = ChevronsInPath( path );
            var totalDistance = path.Distance;
            var safeDistance = totalDistance - (dot_distance * 2);
            var distancePerChevron = safeDistance / chevronCount;

            SlideSegment currentSegment = segmentPool.Get();
            var reverseSegments = new List<SlideSegment> { currentSegment };

            float lastAngle = path.PositionAt(0).GetDegreesFromPosition(path.PositionAt(0.1 / totalDistance));
            for (int i = 0; i < chevronCount; i++)
            {
                var progress = (double)i / (chevronCount - 1); // from 0 to 1, both inclusive

                var position = path.PositionAt(((progress * safeDistance) + dot_distance) / totalDistance);
                var nextPosition = path.PositionAt(((progress * safeDistance) + dot_distance + 0.1) / totalDistance);
                var angle = position.GetDegreesFromPosition(nextPosition);

                currentSegment.Add(chevronPool.Get().With(c => {
                    c.Position = position;
                    c.Rotation = angle;
                    c.ShouldHide = SentakkiExtensions.GetDeltaAngle( lastAngle, angle ) >= 89;
                    c.Alpha = c.ShouldHide ? 0 : 1;
                    c.Depth = i; // earlier ones should remain on top
                }));

                // add next segment if not last ( last segment has up to 5 chevrons while all other 3 )
                if ( i % 3 == 2 && chevronCount - 2 > i )
                {
                    currentSegment = segmentPool.Get();
                    reverseSegments.Add(currentSegment);
                }

                lastAngle = angle;
            }
            segments.AddRange( reverseSegments.Reverse<SlideSegment>() );
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
                int chevrons = (int)Math.Ceiling(Path.Distance / distance_per_chevron);
                double fadeDuration = duration / chevrons;
                double currentOffset = duration / 2;
                for (int i = segments.Count - 1; i >= 0; i--)
                {
                    var segment = segments[i];
                    for (int j = segment.Children.Count - 1; j >= 0; j--)
                    {
                        var chevron = segment.Children[j] as SlideChevron;
                        if (!chevron.ShouldHide)
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
            int chevronsLeft = (int)Math.Ceiling(Path.Distance / distance_per_chevron);
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
                    if (!chevron.ShouldHide)
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
            public bool ShouldHide;

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
