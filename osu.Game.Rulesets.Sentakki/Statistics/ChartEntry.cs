using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Statistics
{
    internal partial class ChartEntry : CompositeDrawable
    {
        private static readonly Color4 accent_color = Color4Extensions.FromHex("#66FFCC");

        private static readonly Color4 background_color = Color4Extensions.FromHex("#202624");

        private const double bar_fill_duration = 3000;

        private readonly string name;

        private readonly IReadOnlyList<HitEvent> hitEvents;
        public ChartEntry(string name, IReadOnlyList<HitEvent> hitEvents)
        {
            this.name = name;
            this.hitEvents = hitEvents;
        }

        private SimpleStatsSegment simpleStats = null!;
        private Drawable detailedStats = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.Both;
            Height = 1f / 6f;
            Scale = new Vector2(1, 0);

            Masking = true;
            BorderThickness = 2;
            BorderColour = accent_color;
            CornerRadius = 5;
            CornerExponent = 2.5f;

            Alpha = hitEvents.Any() ? 1 : 0.8f;
            Colour = !hitEvents.Any() ? Color4.DarkGray : Color4.White;

            bool allPerfect = hitEvents.Any() && hitEvents.All(h => h.Result == HitResult.Great);

            var bg = background_color;

            if (allPerfect)
                bg = Interpolation.ValueAt(0.1, bg, accent_color, 0, 1);

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = bg,
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new Dimension[]{
                        new Dimension(GridSizeMode.Distributed, minSize: 10, maxSize:30),
                        new Dimension(GridSizeMode.AutoSize), // Left text
                        new Dimension(GridSizeMode.Distributed, minSize: 5, maxSize: 30),
                        new Dimension(GridSizeMode.Distributed), // Bars
                        new Dimension(GridSizeMode.Distributed, minSize: 5, maxSize: 30),
                        new Dimension(GridSizeMode.AutoSize), // Total count
                        new Dimension(GridSizeMode.AutoSize), // Detailed count
                        new Dimension(GridSizeMode.Distributed, minSize: 10, maxSize:30),
                    },
                    Content = new[]{
                        new Drawable[]{
                            null!,
                            new Container
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(102,20),
                                Child = new OsuSpriteText
                                {
                                    Colour = accent_color,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = name.ToUpper(),
                                    Font = OsuFont.Torus.With(size: 20, weight: FontWeight.Bold),
                                },
                            },
                            null!, // Container only
                            simpleStats = new SimpleStatsSegment(hitEvents),
                            null!,
                            new ResultsCounter("Total", hitEvents.Count)
                            {
                                Colour = accent_color ,
                            },
                            detailedStats = new DetailedStatsSegment(hitEvents) {
                                Scale = new Vector2(0,1),
                                Margin = new MarginPadding{ Left = 15 }
                            },
                            null!,
                        }
                    }
                },
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            detailedStats.ScaleTo(Vector2.One, 200, Easing.OutElasticQuarter);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            detailedStats.ScaleTo(new Vector2(0, 1), 200, Easing.OutExpo);
        }


        public void AnimateEntry(double entryDuration)
        {
            this.ScaleTo(1, entryDuration, Easing.OutBack);
            simpleStats.AnimateEntry();
        }

        private partial class SimpleStatsSegment : GridContainer
        {
            private Container ratioBoxes;
            private IReadOnlyList<HitEvent> hitEvents;

            public SimpleStatsSegment(IReadOnlyList<HitEvent> hitEvents)
            {
                this.hitEvents = hitEvents;
                RelativeSizeAxes = Axes.Both;
                Content = new[]{
                    new Drawable[]{
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            CornerRadius = 5,
                            CornerExponent = 2.5f,
                            Masking = true,
                            BorderThickness = 2,
                            BorderColour = accent_color,
                            Height = 0.8f,
                            Children = new Drawable[]{
                                new Box
                                {
                                    Alpha = 0,
                                    AlwaysPresent = true,
                                    RelativeSizeAxes = Axes.Both
                                },
                                ratioBoxes = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Size = new Vector2(0,1),
                                }
                            }
                        },
                    }
                };

                if (!hitEvents.Any()) return;

                addRatioBoxFor(HitResult.Ok);
                addRatioBoxFor(HitResult.Good);
                addRatioBoxFor(HitResult.Great);
            }

            public void AnimateEntry()
            {
                ratioBoxes.ResizeWidthTo(1, bar_fill_duration, Easing.OutPow10);
            }

            // This will add a box for each valid sentakki HitResult, excluding those that aren't visible
            private void addRatioBoxFor(HitResult result)
            {
                int resultCount = hitEvents.Count(e => e.Result >= result);

                if (resultCount == 0) return;

                ratioBoxes.Add(new RatioBox
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = (float)resultCount / hitEvents.Count,
                    Colour = result.GetColorForSentakkiResult(),
                    Alpha = .8f
                });
            }
        }

        public partial class DetailedStatsSegment : FillFlowContainer
        {
            private static readonly HitResult[] valid_results = new HitResult[]{
                HitResult.Great,
                HitResult.Good,
                HitResult.Ok,
                HitResult.Miss
            };

            public DetailedStatsSegment(IReadOnlyList<HitEvent> hitEvents)
            {
                Anchor = Origin = Anchor.Centre;

                AutoSizeAxes = Axes.Both;
                Spacing = new Vector2(10f, 0f);
                Direction = FillDirection.Horizontal;

                foreach (var resultType in valid_results)
                {
                    int amount = hitEvents.Count(e => e.Result == resultType);
                    var colour = resultType.GetColorForSentakkiResult();
                    var hspa = new HSPAColour(colour) { P = 0.6f }.ToColor4();
                    AddInternal(new ResultsCounter(resultType.GetDisplayNameForSentakkiResult(), amount)
                    {
                        Colour = Interpolation.ValueAt(0.5f, colour, hspa, 0, 1),
                        Scale = new Vector2(0.8f)
                    });
                }
            }
        }

        private partial class ResultsCounter : FillFlowContainer
        {
            public ResultsCounter(string title, int count)
            {
                AutoSizeAxes = Axes.Both;
                Spacing = new Vector2(0, -5);
                Direction = FillDirection.Vertical;
                Anchor = Origin = Anchor.Centre;

                AddRangeInternal(new Drawable[]
                {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = title.ToUpper(),
                            Font = OsuFont.Torus.With(size: 20, weight: FontWeight.Bold)
                        },
                        new TotalNoteCounter(count){
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                });
            }
        }

        public partial class TotalNoteCounter : RollingCounter<int>
        {
            protected override double RollingDuration => bar_fill_duration;

            protected override Easing RollingEasing => Easing.OutPow10;

            protected override LocalisableString FormatCount(int count) => count.ToString("N0");

            private int totalValue;

            public TotalNoteCounter(int value)
            {
                Current = new Bindable<int> { Value = 0 };
                totalValue = value;
            }

            protected override OsuSpriteText CreateSpriteText() => new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold),
            };

            protected override void LoadComplete()
            {
                base.LoadComplete();
                this.TransformBindableTo(Current, totalValue);
            }
        }

        private partial class RatioBox : Sprite, ITexturedShaderDrawable
        {
            public new IShader TextureShader { get; private set; } = null!;

            protected override DrawNode CreateDrawNode() => new RatioBoxDrawNode(this);

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaders, IRenderer renderer)
            {
                Texture = renderer.WhitePixel.Crop(new Framework.Graphics.Primitives.RectangleF(0, 0, 1f, 1f), Axes.None, WrapMode.Repeat, WrapMode.Repeat);
                TextureRelativeSizeAxes = Axes.None;
                TextureRectangle = new Framework.Graphics.Primitives.RectangleF(0, 0, 50, 50);

                try
                {
                    TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "DiagonalLinePattern");
                }
                catch // Fallback
                {
                    TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
                }
            }

            private class RatioBoxDrawNode : SpriteDrawNode
            {
                public RatioBoxDrawNode(Sprite source) : base(source) { }
                protected override bool CanDrawOpaqueInterior => false;
            }
        }
    }
}
