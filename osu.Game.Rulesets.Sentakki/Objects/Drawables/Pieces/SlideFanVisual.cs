using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

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

        private Container<SlideFanChevron> chevrons;

        private readonly BindableBool snakingIn = new BindableBool(true);

        public SlideFanVisual()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;
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

            const double endpoint_distance = 80; // margin for each end

            for (int i = 1; i < 12; ++i)
            {
                float progress = (i + 1) / (float)12;
                float scale = progress;
                chevrons.Add(new SlideFanChevron(scale, scale)
                {
                    Y = ((SentakkiPlayfield.RINGSIZE + 50 - (float)endpoint_distance) * scale) - 350,
                    Progress = i / (float)11,
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

                foreach (var chevron in chevrons)
                {
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

            foreach (var chevron in chevrons)
            {
                if (chevron.Alpha == 0)
                    continue;

                chevron.Delay(currentOffset).FadeOut(fadeDuration * 2);
                currentOffset += fadeDuration / 2;
            }
        }

        public class SlideFanChevron : BufferedContainer
        {
            public double Progress;

            public SlideFanChevron(float lengthScale, float HeightScale) : base(cachedFrameBuffer: true)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                AutoSizeAxes = Axes.Both;

                float chevHeight = 6 + 10 + (10 * HeightScale);
                float chevWidth = 6 + (210 * lengthScale);

                AddInternal(new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]{
                        // Outlines
                        new Container
                        {
                            X = 2.5f,
                            Masking = true,
                            CornerRadius = chevHeight/4,
                            CornerExponent = 2.5f,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomRight,
                            Rotation = 22.5f,
                            Width = chevWidth,
                            Height = chevHeight,
                            Child = new Box{
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Gray
                            },
                        },
                        new Container
                        {
                            X = -2.5f,
                            Masking = true,
                            CornerRadius = chevHeight/4,
                            CornerExponent = 2.5f,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomLeft,
                            Rotation = -22.5f,
                            Width = chevWidth,
                            Height = chevHeight,
                            Child = new Box{
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Gray
                            },
                        },
                        // Inners
                        new Container{
                            X = 2.5f,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomRight,
                            Size = new Vector2(chevWidth, chevHeight),
                            Rotation = 22.5f,
                            Padding = new MarginPadding(2),
                            Child = new Container{
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,

                                CornerRadius = (chevHeight-4)/4,
                                CornerExponent = 2.5f,
                                Colour = Color4.White,
                                Child = new Box{
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.White
                                }
                            },
                        },
                        new Container{
                            X = -2.5f,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomLeft,
                            Size = new Vector2(chevWidth, chevHeight),
                            Rotation = -22.5f,
                            Padding = new MarginPadding(2),
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                CornerRadius = (chevHeight-4)/4,
                                CornerExponent = 2.5f,
                                Child = new Box{
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.White
                                }
                            },
                        },
                    }
                });
            }

            public void UpdateProgress(double progress)
            {
                Alpha = progress >= Progress ? 0 : 1;
            }
        }
    }
}
