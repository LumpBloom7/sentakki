using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Configuration;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class SlideVisual : CompositeDrawable
    {
        // This will be proxied, so a must.
        public override bool RemoveWhenNotAlive => false;

        private float progress;
        public float Progress
        {
            get => progress;
            set
            {
                progress = value;
                updateProgress(progress);
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
                updateProgress(progress);
            }
        }

        private Container<SlideSegment> segments;
        private DrawablePool<SlideSegment> segmentPool;
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
                segmentPool = new DrawablePool<SlideSegment>(15),
                chevronPool = new DrawablePool<SlideChevron>(74),
                segments = new Container<SlideSegment>(),
            });
        }

        private double chevronInterval;
        private void updateVisuals()
        {
            foreach (var segment in segments)
                segment.ClearChevrons();
            segments.Clear(false);

            var distance = Path.Distance;
            int chevrons = (int)Math.Ceiling(distance / SlideBody.SLIDE_CHEVRON_DISTANCE);
            chevronInterval = 1.0 / chevrons;

            float? prevAngle = null;
            SlideSegment currentSegment = segmentPool.Get();

            // We add the chevrons starting from the last, so that earlier ones remain on top
            for (double i = chevrons - 1; i > 0; --i)
            {
                Vector2 prevPos = Path.PositionAt((i - 1) * chevronInterval);
                Vector2 currentPos = Path.PositionAt(i * chevronInterval);

                float angle = prevPos.GetDegreesFromPosition(currentPos);
                bool shouldHide = SentakkiExtensions.GetDeltaAngle(prevAngle ?? angle, angle) >= 89;
                prevAngle = angle;

                currentSegment.Add(chevronPool.Get().With(c =>
                {
                    c.Position = currentPos;
                    c.Rotation = angle;
                    c.Alpha = shouldHide ? 0 : 1;
                    c.ShouldHide = shouldHide;
                }));

                if (i % 5 == 0 && chevrons - 1 - i > 2)
                {
                    segments.Add(currentSegment);
                    currentSegment = segmentPool.Get();
                }
            }

            segments.Add(currentSegment);
        }
        private void updateProgress(float progress)
        {
            double segmentBounds = -chevronInterval;

            for (int i = segments.Count - 1; i >= 0; i--)
            {
                var segment = segments[i];
                segmentBounds += segment.ChevronCount * chevronInterval;
                segment.Alpha = (progress > segmentBounds) ? 0 : 1;
            }
        }

        public void PerformEntryAnimation(double duration)
        {
            if (snakingIn.Value)
            {
                int chevrons = (int)Math.Ceiling(Path.Distance / SlideBody.SLIDE_CHEVRON_DISTANCE);
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
                    Texture = textures.Get("slide")
                });
            }
        }
    }
}
