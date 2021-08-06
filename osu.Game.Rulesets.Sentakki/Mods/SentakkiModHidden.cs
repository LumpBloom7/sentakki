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
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModHidden : ModHidden, IApplicableToDrawableRuleset<SentakkiHitObject>
    {
        public override string Description => "Notes fade out just before you hit them.";

        public override double ScoreMultiplier => 1.06;

        public void ApplyToDrawableRuleset(DrawableRuleset<SentakkiHitObject> drawableRuleset)
        {
            SentakkiPlayfield sentakkiPlayfield = (SentakkiPlayfield)drawableRuleset.Playfield;
            LanedPlayfield lanedPlayfield = sentakkiPlayfield.LanedPlayfield;

            var lanedHitObjectArea = lanedPlayfield.LanedHitObjectArea;
            var lanedNoteProxyContainer = lanedHitObjectArea.Child;

            lanedHitObjectArea.Remove(lanedNoteProxyContainer);
            lanedHitObjectArea.Add(new PlayfieldMaskingContainer(lanedNoteProxyContainer)
            {
                CoverageRadius = 0.6f
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
                    sb.SlideStar.Hide();

                    preemptTime = sb.HitObject.StartTime - sb.LifetimeStart;
                    fadeOutTime = sb.HitObject.Duration + preemptTime;
                    using (sb.BeginAbsoluteSequence(sb.HitObject.StartTime - preemptTime))
                        sb.Slidepath.FadeOut(fadeOutTime);
                    break;
            }
        }

        private class PlayfieldMaskingContainer : CompositeDrawable
        {
            private readonly PlayfieldMask cover;

            public PlayfieldMaskingContainer(Drawable content)
            {
                RelativeSizeAxes = Axes.Both;

                InternalChild = new BufferedContainer
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
                set => cover.MaskRadius = new Vector2(0, SentakkiPlayfield.RINGSIZE * value / 4);
            }

            private class PlayfieldMask : Drawable
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

                public Vector2 MaskPosition => ToParentSpace(OriginPosition);

                private class PlayfieldMaskDrawNode : DrawNode
                {
                    protected new PlayfieldMask Source => (PlayfieldMask)base.Source;

                    private IShader shader;
                    private Quad screenSpaceDrawQuad;

                    private Vector2 maskPosition;
                    private Vector2 maskRadius;

                    private readonly VertexBatch<PositionAndColourVertex> quadBatch = new QuadBatch<PositionAndColourVertex>(1, 1);
                    private readonly Action<TexturedVertex2D> addAction;

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

                    public override void Draw(Action<TexturedVertex2D> vertexAction)
                    {
                        base.Draw(vertexAction);

                        shader.Bind();

                        shader.GetUniform<Vector2>("maskPosition").UpdateValue(ref maskPosition);
                        shader.GetUniform<Vector2>("maskRadius").UpdateValue(ref maskRadius);

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
}
