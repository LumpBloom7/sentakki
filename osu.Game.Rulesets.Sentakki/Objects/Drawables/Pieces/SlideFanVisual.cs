using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class SlideFanVisual : CompositeDrawable
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
        private SentakkiSlidePath path;

        public SentakkiSlidePath Path
        {
            get => path;
            set
            {
                path = value;
                updateVisuals();
                updateProgress();
            }
        }

        private Container<SlideFanChevron> chevrons;

        private readonly BindableBool snakingIn = new BindableBool(true);

        public SlideFanVisual()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }


        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfig)
        {
            sentakkiConfig?.BindWith(SentakkiRulesetSettings.SnakingSlideBody, snakingIn);

            AddRangeInternal(new Drawable[]{
                chevrons = new Container<SlideFanChevron>(){
                    Alpha = 0.75f
                },
            });
        }

        private const int chevrons_per_eith = 8;
        private const double ring_radius = 297;
        private const double chevrons_per_distance = (chevrons_per_eith * 8) / (2 * Math.PI * ring_radius);
        private const double endpoint_distance = 70; // margin for each end
        private const float spacing = 41;

        private static int chevronsInContinuousPath(SliderPath path)
        {
            return (int)Math.Ceiling((path.Distance - (2 * endpoint_distance)) * chevrons_per_distance);
        }

        private void updateVisuals()
        {
            //float currentY = -300;
            for (int i = 0; i < 18; ++i)
            {
                float progress = (i + 1) / (float)18;
                float scale = (float)Interpolation.ApplyEasing(Easing.InQuad, progress);
                if (scale < 0.03)
                    continue;
                chevrons.Add(new SlideFanChevron
                {
                    Y = (SentakkiPlayfield.RINGSIZE - (float)endpoint_distance) * scale - 300,
                    Scale = new Vector2(scale),
                    Progress = (chevrons.Count + 1) / (float)18,
                });
            }
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
            if (snakingIn.Value)
            {
                double fadeDuration = duration / chevrons.Count;
                double currentOffset = duration / 2;
                for (int j = chevrons.Count - 1; j >= 0; j--)
                {
                    var chevron = chevrons[j];
                    chevron.FadeOut().Delay(currentOffset).FadeIn(fadeDuration * 2).Finally(finalSteps);
                    currentOffset += fadeDuration / 2;
                }
            }
            else
            {
                chevrons.FadeOut().Delay(duration / 2).FadeIn(duration / 2);
            }

            void finalSteps(SlideFanChevron chevron) => chevron.UpdateProgress(progress);
        }

        public void PerformExitAnimation(double duration)
        {
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

        public void Free()
        {
            chevrons.Clear(false);
        }

        public class SlideFanChevron : CompositeDrawable
        {
            public double Progress;

            public SlideFanChevron()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Rotation = 180;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                AddInternal(new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = textures.Get("FanSlide"),
                });
            }

            public void UpdateProgress(double progress)
            {
                Alpha = progress >= Progress ? 0 : 1;
            }
        }
    }
}
