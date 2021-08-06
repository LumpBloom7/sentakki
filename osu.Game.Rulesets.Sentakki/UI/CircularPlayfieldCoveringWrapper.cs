using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics.OpenGL.Vertices;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public enum VisibilityReductionMode
    {
        Hidden,
        FadeIn,
    }

    public class CircularPlayfieldCoveringWrapper : CompositeDrawable
    {
        private readonly PlayfieldCover cover;

        public CircularPlayfieldCoveringWrapper(Drawable content, VisibilityReductionMode mode)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = new BufferedContainer
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new[]
                {
                    new Container
                    {
                        Size = new Vector2(SentakkiPlayfield.RINGSIZE),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Child = content
                    },
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Blending = new BlendingParameters
                        {
                            // Don't change the destination colour.
                            RGBEquation = BlendingEquation.Add,
                            Source = BlendingType.Zero,
                            Destination = BlendingType.One,
                            // Subtract the cover's alpha from the destination (points with alpha 1 should make the destination completely transparent).
                            AlphaEquation = BlendingEquation.Add,
                            SourceAlpha = BlendingType.Zero,
                            DestinationAlpha = mode == VisibilityReductionMode.Hidden ? BlendingType.OneMinusSrcAlpha : BlendingType.SrcAlpha
                        },
                        Child = cover = new PlayfieldCover()
                    }
                }
            };
        }

        /// <summary>
        /// The relative area that should be completely covered if it is FadingIn, or the visible area if it is Hidden.
        /// Doesn't include fade
        /// </summary>
        public float CoverageRadius
        {
            set => cover.ApertureSize = new Vector2(0, SentakkiPlayfield.RINGSIZE / 2 * value);
        }

        private class PlayfieldCover : Drawable
        {
            private IShader shader;

            protected override DrawNode CreateDrawNode() => new PlayfieldMaskDrawNode(this);

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaderManager)
            {
                RelativeSizeAxes = Axes.Both;
                Anchor = Origin = Anchor.Centre;
                shader = shaderManager.Load("PositionAndColour", "PlayfieldMask");
            }

            private Vector2 apertureSize;

            public Vector2 ApertureSize
            {
                get => apertureSize;
                set
                {
                    if (apertureSize == value) return;

                    apertureSize = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            public Vector2 AperturePosition => ToParentSpace(OriginPosition);

            private class PlayfieldMaskDrawNode : DrawNode
            {
                protected new PlayfieldCover Source => (PlayfieldCover)base.Source;

                private IShader shader;
                private Quad screenSpaceDrawQuad;

                private Vector2 aperturePosition;
                private Vector2 apertureSize;

                private readonly VertexBatch<PositionAndColourVertex> quadBatch = new QuadBatch<PositionAndColourVertex>(1, 1);
                private readonly Action<TexturedVertex2D> addAction;

                public PlayfieldMaskDrawNode(PlayfieldCover source)
                    : base(source)
                {
                    addAction = v => quadBatch.Add(new PositionAndColourVertex
                    {
                        Position = v.Position,
                        Colour = v.Colour
                    });
                }

                public override void ApplyState()
                {
                    base.ApplyState();

                    shader = Source.shader;
                    screenSpaceDrawQuad = Source.ScreenSpaceDrawQuad;
                    aperturePosition = Vector2Extensions.Transform(Source.AperturePosition, DrawInfo.Matrix);
                    apertureSize = Source.ApertureSize * DrawInfo.Matrix.ExtractScale().Xy;
                }

                public override void Draw(Action<TexturedVertex2D> vertexAction)
                {
                    base.Draw(vertexAction);

                    shader.Bind();

                    shader.GetUniform<Vector2>("aperturePos").UpdateValue(ref aperturePosition);
                    shader.GetUniform<Vector2>("apertureSize").UpdateValue(ref apertureSize);

                    DrawQuad(Texture.WhitePixel, screenSpaceDrawQuad, DrawColourInfo.Colour, vertexAction: addAction);

                    shader.Unbind();
                }

                protected override void Dispose(bool isDisposing)
                {
                    base.Dispose(isDisposing);
                    quadBatch?.Dispose();
                }
            }
        }
    }
}
