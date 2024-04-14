using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Layout;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides
{
    /// <summary>
    /// This drawable holds a set of all chevron buffered drawables, and is used to preload all/draw of them outside of playfield. (To avoid Playfield transforms re-rendering the chevrons)
    /// <br/>
    /// A view of each chevron, along with their size, would be used by SlideFanVisual.
    /// </summary>
    public partial class SlideFanChevrons : CompositeDrawable
    {
        private Container<ChevronBackingTexture> chevrons = null!;

        public SlideFanChevrons()
        {
            Alpha = 0;
            AlwaysPresent = true;

            // we are doing this in ctor to guarantee that this object is properly initialized before BDL
            loadChevronsTextures();
        }

        public (BufferedContainerView<Drawable>, IBindable<Vector2>) Get(int index)
        {
            var chevron = chevrons[index];

            var view = chevron.CreateView();
            view.RelativeSizeAxes = Axes.Both;

            return (view, chevron.SizeBindable);
        }

        private void loadChevronsTextures()
        {
            AddInternal(chevrons = new Container<ChevronBackingTexture>());

            for (int i = 0; i < 11; ++i)
            {
                float progress = (i + 2) / (float)12;
                float scale = progress;

                chevrons.Add(new ChevronBackingTexture(scale, scale));
            }
        }

        private partial class ChevronBackingTexture : BufferedContainer
        {
            public Bindable<Vector2> SizeBindable { get; } = new Bindable<Vector2>();

            // This is to ensure that drawables using this texture is sized correctly (since autosize only happens during the first update)
            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                if (invalidation == Invalidation.DrawSize)
                    SizeBindable.Value = DrawSize;

                return base.OnInvalidate(invalidation, source);
            }

            private float chevHeight, chevWidth;

            public ChevronBackingTexture(float lengthScale, float heightScale)
                : base(cachedFrameBuffer: true)
            {
                chevHeight = 16 + (10 * heightScale);
                chevWidth = 6 + (210 * lengthScale);
                AutoSizeAxes = Axes.Both;

                // Effect container
                var effectWrapper = new BufferedContainer(cachedFrameBuffer: true)
                {
                    BlurSigma = new Vector2(20),
                    EffectColour = Color4.White,
                    EffectBlending = BlendingParameters.Additive,
                    EffectPlacement = EffectPlacement.Behind,

                    DrawOriginal = true,

                    Padding = new MarginPadding
                    {
                        Horizontal = Blur.KernelSize(25),
                        Vertical = Blur.KernelSize(25),
                    },
                }.Wrap((Drawable)new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                     {
                        // Outlines
                        new Container
                        {
                            X = 2.5f,
                            Masking = true,
                            CornerRadius = chevHeight / 4,
                            CornerExponent = 2.5f,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomRight,
                            Rotation = 22.5f,
                            Width = chevWidth,
                            Height = chevHeight,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Gray
                            },
                        },
                        new Container
                        {
                            X = -2.5f,
                            Masking = true,
                            CornerRadius = chevHeight / 4,
                            CornerExponent = 2.5f,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomLeft,
                            Rotation = -22.5f,
                            Width = chevWidth,
                            Height = chevHeight,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Gray
                            },
                        },
                        // Inners
                        new Container
                        {
                            X = 2.5f,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomRight,
                            Size = new Vector2(chevWidth, chevHeight),
                            Rotation = 22.5f,
                            Padding = new MarginPadding(2),
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,

                                CornerRadius = (chevHeight - 4) / 4,
                                CornerExponent = 2.5f,
                                Colour = Color4.White,
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.White
                                }
                            },
                        },
                        new Container
                        {
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
                                CornerRadius = (chevHeight - 4) / 4,
                                CornerExponent = 2.5f,
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.White
                                }
                            },
                        },
                     }
                });

                AddInternal(effectWrapper);
            }
        }
    }
}
