using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects;
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
                path = value;
                ClearInternal();
                createVisuals();
                updateProgress(progress);
            }
        }

        public SlideVisual()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        private List<Container> segments = new List<Container>();

        private double chevronInterval;
        private void createVisuals()
        {
            segments = new List<Container>();
            var distance = Path.Distance;
            int chevrons = (int)Math.Ceiling(distance / SlideBody.SLIDE_CHEVRON_DISTANCE);
            chevronInterval = 1.0 / chevrons;

            float? prevAngle = null;
            Container currentSegment = new Container();

            // We add the chevrons starting from the last, so that earlier ones remain on top
            for (double i = chevrons - 1; i > 0; --i)
            {
                Vector2 prevPos = Path.PositionAt((i - 1) * chevronInterval);
                Vector2 currentPos = Path.PositionAt(i * chevronInterval);

                float angle = prevPos.GetDegreesFromPosition(currentPos);
                bool shouldHide = SentakkiExtensions.GetDeltaAngle(prevAngle ?? angle, angle) >= 89;
                prevAngle = angle;

                currentSegment.Add(new SlideChevron
                {
                    Position = currentPos,
                    Rotation = angle,
                    Alpha = shouldHide ? 0 : 1,
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

            for (int i = segments.Count - 1; i >= 0; i--)
            {
                var segment = segments[i];
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
