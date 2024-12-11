using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Rendering.Vertices;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Sentakki.Configuration;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI.Components
{
    public partial class PlayfieldVisualisation : Drawable, IHasAccentColour
    {
        private readonly IBindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        /// <summary>
        /// The number of bars to jump each update iteration.
        /// </summary>
        private const int index_change = 5;

        /// <summary>
        /// The maximum length of each bar in the visualiser. Will be reduced when kiai is not activated.
        /// </summary>
        private const float bar_length = 600;

        /// <summary>
        /// The number of bars in one rotation of the visualiser.
        /// </summary>
        private const int bars_per_visualiser = 200;

        /// <summary>
        /// How many times we should stretch around the circumference (overlapping overselves).
        /// </summary>
        private const float visualiser_rounds = 5;

        /// <summary>
        /// How much should each bar go down each millisecond (based on a full bar).
        /// </summary>
        private const float decay_per_milisecond = 0.0024f;

        /// <summary>
        /// Number of milliseconds between each amplitude update.
        /// </summary>
        private const float time_between_updates = 50;

        /// <summary>
        /// The minimum amplitude to show a bar.
        /// </summary>
        private const float amplitude_dead_zone = 1f / bar_length;

        private int indexOffset;

        public Color4 AccentColour { get; set; } = Color4.White.Opacity(0.2f);

        private readonly float[] frequencyAmplitudes = new float[256];

        private IShader shader = null!;
        private Texture texture = null!;

        public PlayfieldVisualisation()
        {
            FillAspectRatio = 1;
            FillMode = FillMode.Fit;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(.99f);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Blending = BlendingParameters.Additive;
        }

        private readonly Bindable<bool> kiaiEffect = new Bindable<bool>(true);

        [BackgroundDependencyLoader]
        private void load(IRenderer renderer, ShaderManager shaders, IBindable<WorkingBeatmap> beatmap, SentakkiRulesetConfigManager settings)
        {
            this.beatmap.BindTo(beatmap);
            texture = renderer.WhitePixel;
            shader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);

            settings?.BindWith(SentakkiRulesetSettings.KiaiEffects, kiaiEffect);
            kiaiEffect.BindValueChanged(k =>
            {
                if (k.NewValue)
                    this.FadeIn(200);
                else
                    this.FadeOut(500);
            });
        }

        protected override void LoadComplete()
        {
            kiaiEffect.TriggerChange();
        }

        // Returns true if amplitude have been updated
        private void updateAmplitudes()
        {
            var track = beatmap.Value.TrackLoaded ? beatmap.Value.Track : null;
            var effect = beatmap.Value.BeatmapLoaded ? beatmap.Value.Beatmap?.ControlPointInfo.EffectPointAt(track?.CurrentTime ?? Time.Current) : null;

            if (!effect?.KiaiMode ?? false)
                return;

            ReadOnlySpan<float> temporalAmplitudes = (track?.CurrentAmplitudes ?? ChannelAmplitudes.Empty).FrequencyAmplitudes.Span;

            for (int i = 0; i < bars_per_visualiser; i++)
            {
                float targetAmplitude = temporalAmplitudes[(i + indexOffset) % bars_per_visualiser];
                if (targetAmplitude > frequencyAmplitudes[i])
                    frequencyAmplitudes[i] = targetAmplitude;
            }

            indexOffset = (indexOffset + index_change) % bars_per_visualiser;

            // It's kiai time, turn on the visualisation
            ShouldDraw = true;
        }

        private double timeDelta;

        public bool ShouldDraw { get; private set; }

        protected override void Update()
        {
            base.Update();

            timeDelta += Math.Abs(Time.Elapsed);

            if (timeDelta >= time_between_updates)
            {
                timeDelta %= time_between_updates;
                updateAmplitudes();
            }

            // We don't have to update further if we don't need to draw
            if (!ShouldDraw)
                return;

            float decayFactor = Math.Abs((float)Time.Elapsed) * decay_per_milisecond;
            ShouldDraw = false;

            for (int i = 0; i < bars_per_visualiser; i++)
            {
                //3% of extra bar length to make it a little faster when bar is almost at it's minimum
                frequencyAmplitudes[i] -= decayFactor * (frequencyAmplitudes[i] + 0.03f);
                if (frequencyAmplitudes[i] < 0)
                    frequencyAmplitudes[i] = 0;

                if (frequencyAmplitudes[i] != 0)
                    ShouldDraw = true;
            }

            // Don't invalidate the draw node if we don't plan to draw
            if (!ShouldDraw)
                return;

            Invalidate(Invalidation.DrawNode);
        }

        protected override DrawNode CreateDrawNode() => new VisualisationDrawNode(this);

        private class VisualisationDrawNode : DrawNode
        {
            protected new PlayfieldVisualisation Source => (PlayfieldVisualisation)base.Source;

            private IShader shader = null!;
            private Texture texture = null!;

            // Assuming the logo is a circle, we don't need a second dimension.
            private float size;

            private Color4 colour;
            private float[] audioData = null!;

            private IVertexBatch<TexturedVertex2D> vertexBatch = null!;

            public VisualisationDrawNode(PlayfieldVisualisation source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                shader = Source.shader;
                texture = Source.texture;
                size = Source.DrawSize.X;
                colour = Source.AccentColour;
                audioData = Source.frequencyAmplitudes;
            }

            protected override void Draw(IRenderer renderer)
            {
                if (!Source.ShouldDraw)
                    return;

                base.Draw(renderer);

                vertexBatch ??= renderer.CreateQuadBatch<TexturedVertex2D>(100, 10);

                shader.Bind();

                Vector2 inflation = DrawInfo.MatrixInverse.ExtractScale().Xy;

                ColourInfo colourInfo = DrawColourInfo.Colour;
                colourInfo.ApplyChild(colour);

                if (audioData != null)
                {
                    for (int j = 0; j < visualiser_rounds; j++)
                    {
                        for (int i = 0; i < bars_per_visualiser; i++)
                        {
                            if (audioData[i] < amplitude_dead_zone)
                                continue;

                            float rotation = float.DegreesToRadians((i / (float)bars_per_visualiser * 360) + (j * 360 / visualiser_rounds));
                            float rotationCos = MathF.Cos(rotation);
                            float rotationSin = MathF.Sin(rotation);
                            // taking the cos and sin to the 0..1 range
                            var barPosition = new Vector2((rotationCos / 2) + 0.5f, (rotationSin / 2) + 0.5f) * size;

                            var barSize = new Vector2(size * MathF.Sqrt(2 * (1 - MathF.Cos(float.DegreesToRadians(360f / bars_per_visualiser)))) / 2f, bar_length * audioData[i]);
                            // The distance between the position and the sides of the bar.
                            var bottomOffset = new Vector2(-rotationSin * barSize.X / 2, rotationCos * barSize.X / 2);
                            // The distance between the bottom side of the bar and the top side.
                            var amplitudeOffset = new Vector2(rotationCos * barSize.Y, rotationSin * barSize.Y);

                            var rectangle = new Quad(
                                Vector2Extensions.Transform(barPosition - bottomOffset, DrawInfo.Matrix),
                                Vector2Extensions.Transform(barPosition - bottomOffset + amplitudeOffset, DrawInfo.Matrix),
                                Vector2Extensions.Transform(barPosition + bottomOffset, DrawInfo.Matrix),
                                Vector2Extensions.Transform(barPosition + bottomOffset + amplitudeOffset, DrawInfo.Matrix)
                            );

                            renderer.DrawQuad(
                                texture,
                                rectangle,
                                colourInfo,
                                null,
                                vertexBatch.AddAction,
                                // barSize by itself will make it smooth more in the X axis than in the Y axis, this reverts that.
                                Vector2.Divide(inflation, barSize.Yx));
                        }
                    }
                }

                shader.Unbind();
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                vertexBatch?.Dispose();
            }
        }
    }
}
