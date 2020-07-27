using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects;
using osu.Framework.Allocation;
using System.Collections.Generic;
using System;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class SlideBody : CompositeDrawable
    {
        private float progress = 0;
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
                path = value;
                ClearInternal();
                createVisuals();
                updateProgress(progress);
            }
        }

        public SlideBody()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        private List<Container> segments = new List<Container>();

        private double chevronInterval = 0;
        private void createVisuals()
        {
            segments = new List<Container>();
            var distance = Path.Distance;
            int chevrons = (int)Math.Ceiling(distance / Slide.SLIDE_CHEVRON_DISTANCE);
            chevronInterval = 1.0 / chevrons;

            float prevAngle = 0;
            Container currentSegment = new Container();
            for (double i = 1; i < chevrons; ++i)
            {
                Vector2 currentPos = Path.PositionAt(i * chevronInterval);
                Vector2 nextPos = Path.PositionAt((i + 1) * chevronInterval);
                float angle = currentPos.GetDegreesFromPosition(nextPos);
                if (i == chevrons - 1) angle = prevAngle;
                prevAngle = angle;

                currentSegment.Add(new SlideChevron
                {
                    Position = currentPos,
                    Rotation = angle,
                });

                if (i % 5 == 0 && chevrons - 1 - i > 2)
                {
                    segments.Add(currentSegment);
                    currentSegment = new Container();
                }
            }
            segments.Add(currentSegment);
            AddRangeInternal(segments);
        }
        private void updateProgress(float progress)
        {
            double segmentBounds = -chevronInterval;

            foreach (var segment in segments)
            {
                segmentBounds += segment.Count * chevronInterval;
                segment.Alpha = (progress > segmentBounds) ? 0 : 1;
            }
        }

        private class SlideChevron : Sprite
        {
            public SlideChevron()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }
            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = textures.Get("slide");
            }
        }
    }
}