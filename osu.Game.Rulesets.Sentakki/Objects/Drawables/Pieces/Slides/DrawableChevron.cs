using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;

public partial class DrawableChevron : Sprite, ITexturedShaderDrawable
{
    private float thickness = 13f;

    public float Thickness
    {
        get => thickness;
        set
        {
            if (thickness == value)
                return;

            thickness = value;
            Invalidate(Invalidation.DrawNode);
        }
    }

    private float shadowRadius = 7.5f;

    public float ShadowRadius
    {
        get => shadowRadius;
        set
        {
            if (shadowRadius == value)
                return;

            shadowRadius = value;
            Invalidate(Invalidation.DrawNode);
        }
    }

    private bool glow;

    public bool Glow
    {
        get => glow;
        set
        {
            if (glow == value)
                return;

            glow = value;
            Invalidate(Invalidation.DrawNode);
        }
    }

    private bool fanChevron;

    public bool FanChevron
    {
        get => fanChevron;
        set
        {
            if (fanChevron == value)
                return;

            fanChevron = value;
            Invalidate(Invalidation.DrawNode);
        }
    }

    public new IShader TextureShader { get; private set; } = null!;

    protected override DrawNode CreateDrawNode() => new ChevronDrawNode(this);

    [BackgroundDependencyLoader]
    private void load(ShaderManager shaders, IRenderer renderer)
    {
        TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "chevron");
        Texture = renderer.WhitePixel;

        Blending = new BlendingParameters
        {
            Source = BlendingType.One,
            Destination = BlendingType.OneMinusSrcAlpha,

            SourceAlpha = BlendingType.One,
            DestinationAlpha = BlendingType.OneMinusSrcAlpha,

            RGBEquation = BlendingEquation.Add,
            AlphaEquation = BlendingEquation.Add
        };
    }

    private partial class ChevronDrawNode : SpriteDrawNode
    {
        protected new DrawableChevron Source => (DrawableChevron)base.Source;
        protected override bool CanDrawOpaqueInterior => false;

        private IUniformBuffer<ShapeParameters>? uniformBuffer;
        private static readonly List<IUniformBuffer<ShapeParameters>> shared_uniform_buffers = [];

        private ShapeParameters parameters;

        public ChevronDrawNode(DrawableChevron source)
            : base(source)
        {
        }

        public override void ApplyState()
        {
            base.ApplyState();
            var newParameters = new ShapeParameters()
            {
                Thickness = Source.thickness,
                Size = Source.DrawSize,
                ShadowRadius = Source.ShadowRadius,
                Glow = Source.glow,
                FanChevron = Source.FanChevron,
            };

            // If the uniform properties have changed, then we definitely want to null this out so that we get a more appropriate uniform block
            // We don't care about disposal since these uniform blocks are shared
            if (newParameters == parameters) return;

            // If the uniform properties have changed, then we definitely want to null this out so that we get a more appropriate uniform block
            // We don't care about disposal since these uniform blocks are shared
            parameters = newParameters;
            uniformBuffer = null;
        }

        protected override void BindUniformResources(IShader shader, IRenderer renderer)
        {
            base.BindUniformResources(shader, renderer);

            uniformBuffer ??= shared_uniform_buffers.FirstOrDefault(isMatchingUniformBlock);

            if (uniformBuffer is null)
            {
                uniformBuffer = renderer.CreateUniformBuffer<ShapeParameters>();
                shared_uniform_buffers.Add(uniformBuffer);
            }

            uniformBuffer.Data = parameters;

            shader.BindUniformBlock("m_shapeParameters", uniformBuffer);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            // For now we always share the parameters, so no need to dispose
            //shapeParameters?.Dispose();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private record struct ShapeParameters
        {
            public UniformFloat Thickness;
            public UniformPadding4 _;
            public UniformVector2 Size;
            public UniformFloat ShadowRadius;
            public UniformBool Glow;
            public UniformBool FanChevron;

            public UniformPadding4 __;
        }

        private bool isMatchingUniformBlock(IUniformBuffer<ShapeParameters> uniformBuffer)
        {
            return uniformBuffer.Data == parameters;
        }
    }
}
