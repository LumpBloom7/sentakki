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

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;

public enum NoteShape
{
    Ring,
    Hex,
    Star
}

public partial class LaneNoteVisual : Sprite, ITexturedShaderDrawable
{
    // Expectation: The shape will never change once set;
    private bool useSharedUniformBuffer => Shape is not NoteShape.Hex;
    public NoteShape Shape { get; init; } = NoteShape.Ring;
    private float thickness = 18.75f;

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

    private float shadowRadius = 15;

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

    public new IShader TextureShader { get; private set; } = null!;

    protected override DrawNode CreateDrawNode() => new LaneNoteVisualDrawNode(this);

    private readonly BindableBool exBindable = new BindableBool();

    private string fragmentShaderFor(NoteShape shape)
    {
        switch (shape)
        {
            case NoteShape.Ring:
            default:
                return "ringNote";

            case NoteShape.Hex:
                return "hexNote";

            case NoteShape.Star:
                return "starNote";
        }
    }

    [BackgroundDependencyLoader]
    private void load(ShaderManager shaders, IRenderer renderer, DrawableHitObject? hitObject)
    {
        TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, fragmentShaderFor(Shape));
        Texture = renderer.WhitePixel;

        if (hitObject is null)
            return;

        // Bind exnote
        exBindable.BindTo(((DrawableSentakkiHitObject)hitObject).ExBindable);
        exBindable.BindValueChanged(b => Glow = b.NewValue, true);

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

    private partial class LaneNoteVisualDrawNode : SpriteDrawNode
    {
        protected new LaneNoteVisual Source => (LaneNoteVisual)base.Source;
        protected override bool CanDrawOpaqueInterior => false;

        private IUniformBuffer<ShapeParameters>? uniformBuffer;
        private static readonly List<IUniformBuffer<ShapeParameters>> shared_uniform_buffers = [];

        private ShapeParameters parameters;

        public LaneNoteVisualDrawNode(LaneNoteVisual source)
            : base(source)
        {
        }

        public override void ApplyState()
        {
            base.ApplyState();

            var newParameters = new ShapeParameters()
            {
                BorderThickness = Source.Thickness,
                Size = Source.DrawSize,
                ShadowRadius = Source.ShadowRadius,
                Glow = Source.Glow,
            };

            // If the uniform properties have changed, then we definitely want to null this out so that we get a more appropriate uniform block
            // We don't care about disposal since these uniform blocks are shared
            if (newParameters == parameters) return;

            // If the uniform properties have changed, then we definitely want to null this out so that we get a more appropriate uniform block
            // We don't care about disposal since these uniform blocks are shared
            parameters = newParameters;

            if (Source.useSharedUniformBuffer)
                uniformBuffer = null;
        }

        protected override void BindUniformResources(IShader shader, IRenderer renderer)
        {
            base.BindUniformResources(shader, renderer);

            if (Source.useSharedUniformBuffer)
                uniformBuffer ??= shared_uniform_buffers.FirstOrDefault(isMatchingUniformBlock);

            if (uniformBuffer is null)
            {
                uniformBuffer = renderer.CreateUniformBuffer<ShapeParameters>();
                if (Source.useSharedUniformBuffer)
                    shared_uniform_buffers.Add(uniformBuffer);
            }

            uniformBuffer.Data = parameters;

            shader.BindUniformBlock("m_shapeParameters", uniformBuffer);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (!Source.useSharedUniformBuffer)
                uniformBuffer?.Dispose();

            base.Dispose(isDisposing);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private record struct ShapeParameters
        {
            public UniformFloat BorderThickness;
            public UniformPadding4 _;
            public UniformVector2 Size;
            public UniformFloat ShadowRadius;
            public UniformBool Glow;

            public UniformPadding8 __;
        }

        private bool isMatchingUniformBlock(IUniformBuffer<ShapeParameters> uniformBuffer)
        {
            return uniformBuffer.Data == parameters;
        }
    }
}
