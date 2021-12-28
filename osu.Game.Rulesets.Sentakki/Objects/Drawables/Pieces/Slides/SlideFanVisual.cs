using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides
{
    public class SlideFanVisual : SlideVisualBase<SlideFanVisual.SlideFanChevron>
    {
        public SlideFanVisual() : base()
        {
            AutoSizeAxes = Axes.Both;
            Rotation = 22.5f;
        }

        protected override void LoadChevrons()
        {
            const double endpoint_distance = 80; // margin for each end

            for (int i = 11; i > 0; --i)
            {
                float progress = (i + 1) / (float)12;
                float scale = progress;
                Chevrons.Add(new SlideFanChevron(scale, scale)
                {
                    Y = ((SentakkiPlayfield.RINGSIZE + 50 - (float)endpoint_distance) * scale) - 350,
                    Progress = i / (float)11,
                });
            }
        }

        public class SlideFanChevron : BufferedContainer, ISlideChevron
        {
            public double Progress { get; set; }

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
        }
    }
}
