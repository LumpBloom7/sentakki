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
                ClearInternal();
                createVisuals(progress);
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
                createVisuals(progress);
            }
        }

        public SlideBody()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        private void createVisuals(float progress)
        {
            var distance = Path.Distance;
            int chevrons = (int)Math.Ceiling(distance / 25);
            double chevronInterval = 1.0 / chevrons;

            float prevAngle = 0;
            for (double i = chevrons - 1; i >= progress / chevronInterval; --i)
            {
                Vector2 currentPos = Path.PositionAt(i * chevronInterval);
                Vector2 nextPos = Path.PositionAt((i - 1) * chevronInterval);
                float angle = nextPos.GetDegreesFromPosition(currentPos);
                if (i == 0) angle = prevAngle;
                prevAngle = angle;


                AddInternal(new SlideChevron
                {
                    Position = currentPos,
                    Rotation = angle,
                });
            }

        }

        private class SlideChevron : Sprite
        {
            public SlideChevron()
            {
                Scale = new Vector2(1.1f);
                Anchor = Anchor.TopLeft;
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