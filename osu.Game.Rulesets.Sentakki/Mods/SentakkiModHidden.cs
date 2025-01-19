using System;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Rendering.Vertices;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.OpenGL.Vertices;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Localisation.Mods;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public partial class SentakkiModHidden : ModHidden, IApplicableToDrawableRuleset<SentakkiHitObject>
    {
        public override LocalisableString Description => SentakkiModHiddenStrings.ModDescription;

        public override string ExtendedIconInformation => VisibleRadius.IsDefault ? string.Empty : $"{VisibleRadius.Value:P0}";

        public override double ScoreMultiplier => 1 + (MathF.Pow(1 - VisibleRadius.Value, 2) * 0.2);

        [SettingSource(
        typeof(SentakkiModHiddenStrings),
        nameof(SentakkiModHiddenStrings.VisibleRadius),
        nameof(SentakkiModHiddenStrings.VisibleRadiusDescription),
        SettingControlType = typeof(SettingsSlider<float, PercentageRoundedSliderBar>))]
        public BindableFloat VisibleRadius { get; } = new BindableFloat
        {
            MinValue = 0f,
            MaxValue = 1f,
            Default = 0.6f,
            Value = 0.6f,
            Precision = 0.05f
        };


        public void ApplyToDrawableRuleset(DrawableRuleset<SentakkiHitObject> drawableRuleset)
        {
            SentakkiPlayfield sentakkiPlayfield = (SentakkiPlayfield)drawableRuleset.Playfield;
            LanedPlayfield lanedPlayfield = sentakkiPlayfield.LanedPlayfield;

            var lanedHitObjectArea = lanedPlayfield.LanedHitObjectArea;
            var lanedNoteProxyContainer = lanedHitObjectArea.Child;

            float noteVisiblePoint = SentakkiPlayfield.NOTESTARTDISTANCE - (NoteRingPiece.DRAWABLE_SIZE / 2f);
            float totalVisibleDistance = SentakkiPlayfield.INTERSECTDISTANCE;

            float visibilityStartPoint = noteVisiblePoint / totalVisibleDistance;
            float visibleRatio = 1 - visibilityStartPoint;

            lanedHitObjectArea.Remove(lanedNoteProxyContainer, false);
            lanedHitObjectArea.Add(new PlayfieldMaskingContainer(lanedNoteProxyContainer)
            {
                CoverageRadius = visibilityStartPoint + (visibleRatio * VisibleRadius.Value)
            });

            lanedPlayfield.HitObjectLineRenderer.Hide();
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state) => ApplyNormalVisibilityState(hitObject, state);

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            double preemptTime;
            double fadeOutTime;

            switch (hitObject)
            {
                case DrawableTouch t:
                    preemptTime = t.HitObject.HitWindows.WindowFor(HitResult.Ok);
                    fadeOutTime = preemptTime * 0.3f;
                    using (t.BeginAbsoluteSequence(t.HitObject.StartTime - preemptTime))
                        t.TouchBody.FadeOut(fadeOutTime);
                    break;

                case DrawableTouchHold th:
                    th.TouchHoldBody.ProgressPiece.Hide();
                    break;

                case DrawableSlideBody sb:
                    sb.SlideStars.Hide();

                    preemptTime = sb.HitObject.StartTime - sb.LifetimeStart;
                    fadeOutTime = sb.HitObject.Duration + preemptTime;
                    using (sb.BeginAbsoluteSequence(sb.HitObject.StartTime - preemptTime))
                        ((Drawable)sb.Slidepath).FadeOutFromOne(fadeOutTime);
                    break;
            }
        }

        private partial class PercentageRoundedSliderBar : RoundedSliderBar<float>
        {
            public PercentageRoundedSliderBar()
            {
                DisplayAsPercentage = true;
            }
        }

        private partial class PlayfieldMaskingContainer : CircularContainer
        {
            private readonly PlayfieldMask cover;

            public PlayfieldMaskingContainer(Drawable content)
            {
                RelativeSizeAxes = Axes.Both;
                Anchor = Origin = Anchor.Centre;

                // We still enable masking to avoid BufferedContainer visual jank when it rotates
                Masking = true;

                Child = new FixedSizeBufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Child = content
                        },
                        cover = new PlayfieldMask
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
                                DestinationAlpha = BlendingType.OneMinusSrcAlpha
                            },
                        }
                    }
                };
            }

            /// <summary>
            /// The relative visible area of the playfield, half of the distance is used as a fade out.
            /// </summary>
            public float CoverageRadius
            {
                set
                {
                    cover.MaskRadius = new Vector2(0, SentakkiPlayfield.RINGSIZE * value / 4);

                    // We set the buffered container's size to be around the same width of the visible area
                    // This is so that we don't have a buffer bigger than what is needed
                    // If we plan to change the coverage radius often, this will instead be an anti-optimization
                    Size = new Vector2(value);
                }
            }

            // This buffered container maintains a SSDQ unaffected by rotation, so that the backing texture isn't being reallocated due to resizes
            private partial class FixedSizeBufferedContainer : BufferedContainer
            {
                [Resolved]
                private SentakkiPlayfield sentakkiPlayfield { get; set; } = null!;

                protected override Quad ComputeScreenSpaceDrawQuad()
                {
                    var ssdqDrawinfo = DrawInfo;

                    // We apply a counter rotation so that the SSDQ retains the non-rotated Quad
                    ssdqDrawinfo.ApplyTransform(AnchorPosition, Vector2.One, -sentakkiPlayfield.Rotation, Vector2.Zero, OriginPosition);

                    return Quad.FromRectangle(DrawRectangle) * ssdqDrawinfo.Matrix;
                }
            }

            private partial class PlayfieldMask : Drawable
            {
                private IShader shader = null!;

                protected override DrawNode CreateDrawNode() => new PlayfieldMaskDrawNode(this);

                [BackgroundDependencyLoader]
                private void load(ShaderManager shaderManager)
                {
                    RelativeSizeAxes = Axes.Both;
                    Anchor = Origin = Anchor.Centre;
                    shader = shaderManager.Load("PositionAndColour", "PlayfieldMask");
                }

                private Vector2 maskRadius;

                public Vector2 MaskRadius
                {
                    get => maskRadius;
                    set
                    {
                        if (maskRadius == value) return;

                        maskRadius = value;
                        Invalidate(Invalidation.DrawNode);
                    }
                }

                public Vector2 MaskPosition => OriginPosition;

                private class PlayfieldMaskDrawNode : DrawNode
                {
                    protected new PlayfieldMask Source => (PlayfieldMask)base.Source;

                    private IShader shader = null!;
                    private Quad screenSpaceDrawQuad;

                    private IUniformBuffer<MaskParameters>? maskParameters;

                    private Vector2 maskPosition;
                    private Vector2 maskRadius;

                    private IVertexBatch<PositionAndColourVertex> quadBatch = null!;
                    private Action<TexturedVertex2D> addAction;

                    public PlayfieldMaskDrawNode(PlayfieldMask source)
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
                        maskPosition = Vector2Extensions.Transform(Source.MaskPosition, DrawInfo.Matrix);
                        maskRadius = Source.MaskRadius * DrawInfo.Matrix.ExtractScale().Xy;
                    }

                    protected override void Draw(IRenderer renderer)
                    {
                        base.Draw(renderer);

                        maskParameters = renderer.CreateUniformBuffer<MaskParameters>();

                        maskParameters.Data = maskParameters.Data with
                        {
                            MaskPosition = maskPosition,
                            MaskRadius = maskRadius
                        };

                        if (quadBatch == null)
                        {
                            quadBatch = renderer.CreateQuadBatch<PositionAndColourVertex>(1, 1);
                            addAction = v => quadBatch.Add(new PositionAndColourVertex
                            {
                                Position = v.Position,
                                Colour = v.Colour
                            });
                        }

                        shader.Bind();
                        shader.BindUniformBlock("m_maskParameters", maskParameters);

                        renderer.DrawQuad(renderer.WhitePixel, screenSpaceDrawQuad, DrawColourInfo.Colour, vertexAction: addAction);

                        shader.Unbind();
                    }

                    protected override void Dispose(bool isDisposing)
                    {
                        base.Dispose(isDisposing);
                        maskParameters?.Dispose();
                        quadBatch?.Dispose();
                    }


                    [StructLayout(LayoutKind.Sequential, Pack = 1)]
                    private record struct MaskParameters
                    {
                        public UniformVector2 MaskPosition;
                        public UniformVector2 MaskRadius;
                    }
                }
            }
        }
    }
}
