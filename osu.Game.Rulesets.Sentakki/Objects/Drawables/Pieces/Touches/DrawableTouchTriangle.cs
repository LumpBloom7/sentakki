using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Touches;

public partial class DrawableTouchTriangle : Sprite, ITexturedShaderDrawable
{
    private float thickness = 15f;

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

    private float shadowRadius = 15f;

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

    private bool fillTriangle;

    public bool FillTriangle
    {
        get => fillTriangle;
        set
        {
            if (fillTriangle == value)
                return;

            fillTriangle = value;
            Invalidate(Invalidation.DrawNode);
        }
    }

    private bool shadowOnly;

    public bool ShadowOnly
    {
        get => shadowOnly;
        set
        {
            if (shadowOnly == value)
                return;

            shadowOnly = value;
            Invalidate(Invalidation.DrawNode);
        }
    }

    public new IShader TextureShader { get; private set; } = null!;

    protected override DrawNode CreateDrawNode() => new TouchTriangleDrawNode(this);

    private readonly BindableBool exBindable = new BindableBool();

    [BackgroundDependencyLoader]
    private void load(ShaderManager shaders, IRenderer renderer, DrawableHitObject? hitObject)
    {
        TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "touchTriangle");
        Texture = renderer.WhitePixel;

        if (hitObject is null)
            return;

        Blending = new BlendingParameters
        {
            Source = BlendingType.One,
            Destination = BlendingType.OneMinusSrcAlpha,

            SourceAlpha = BlendingType.One,
            DestinationAlpha = BlendingType.OneMinusSrcAlpha,

            RGBEquation = BlendingEquation.Add,
            AlphaEquation = BlendingEquation.Add
        };

        // Bind exnote
        exBindable.BindTo(((DrawableSentakkiHitObject)hitObject).ExBindable);
        exBindable.BindValueChanged(b => Glow = b.NewValue, true);
    }

    private partial class TouchTriangleDrawNode : SpriteDrawNode
    {
        protected new DrawableTouchTriangle Source => (DrawableTouchTriangle)base.Source;
        protected override bool CanDrawOpaqueInterior => false;
        private IUniformBuffer<ShapeParameters>? shapeParameters;

        private static readonly List<IUniformBuffer<ShapeParameters>> shared_shape_parameters = [];

        private float thickness;
        private Vector2 size;
        private bool glow;
        private float shadowRadius;
        private bool fillTriangle;
        private bool shadowOnly;

        public TouchTriangleDrawNode(DrawableTouchTriangle source)
            : base(source)
        {
        }

        public override void ApplyState()
        {
            base.ApplyState();
            thickness = Source.Thickness;
            size = Source.DrawSize;
            shadowRadius = Source.shadowRadius;
            glow = Source.Glow;
            fillTriangle = Source.FillTriangle;
            shadowOnly = Source.ShadowOnly;
        }

        protected override void BindUniformResources(IShader shader, IRenderer renderer)
        {
            base.BindUniformResources(shader, renderer);

            shapeParameters ??= shared_shape_parameters.FirstOrDefault(isMatchingUniformBlock);

            if (shapeParameters is null)
            {
                shapeParameters = renderer.CreateUniformBuffer<ShapeParameters>();
                shared_shape_parameters.Add(shapeParameters);
            }

            shapeParameters.Data = shapeParameters.Data with
            {
                Thickness = thickness,
                Size = size,
                ShadowRadius = shadowRadius,
                Glow = glow,
                fillTriangle = fillTriangle,
                shadowOnly = shadowOnly
            };

            shader.BindUniformBlock("m_shapeParameters", shapeParameters);
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
            public UniformBool fillTriangle;
            public UniformBool shadowOnly;
        }

        private bool isMatchingUniformBlock(IUniformBuffer<ShapeParameters> shapeParameters)
        {
            ShapeParameters data = shapeParameters.Data;
            return data.Thickness == thickness && data.Size == (UniformVector2)size && data.ShadowRadius == shadowRadius && data.Glow == glow && data.fillTriangle == fillTriangle
                   && data.shadowOnly == shadowOnly;
        }
    }
}
