using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;

public enum NoteShape
{
    Ring,
    Hex,
    Star
}

public partial class LaneNoteVisual : Drawable, ITexturedShaderDrawable
{
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

    public IShader TextureShader { get; private set; } = null!;

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
    private void load(ShaderManager shaders, DrawableHitObject? hitObject)
    {
        TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, fragmentShaderFor(Shape));

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

    private partial class LaneNoteVisualDrawNode : TexturedShaderDrawNode
    {
        private Quad screenSpaceDrawQuad { get; set; }
        private Vector2 size { get; set; }

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
            var drawRect = Source.DrawRectangle.Normalize().Inflate(Source.ShadowRadius);
            screenSpaceDrawQuad = Quad.FromRectangle(drawRect) * DrawInfo.Matrix;
            size = drawRect.Size;

            var newParameters = new ShapeParameters()
            {
                BorderThickness = Source.Thickness,
                ShadowRadius = Source.ShadowRadius,
                Glow = Source.Glow,
            };

            // If the uniform properties have changed, then we definitely want to null this out so that we get a more appropriate uniform block
            // We don't care about disposal since these uniform blocks are shared
            if (newParameters == parameters) return;

            uniformBuffer = null;
            parameters = newParameters;
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

        protected override void Draw(IRenderer renderer)
        {
            if (size.X == 0 || size.Y == 0)
                return;

            base.Draw(renderer);

            BindTextureShader(renderer);

            renderer.DrawQuad(renderer.WhitePixel, screenSpaceDrawQuad, DrawColourInfo.Colour,
                null,
                null,
                Vector2.Zero,
                size); // HACK: I use blendRangeOverride to pass in the actual size of the drawable, to avoid using a uniform for it.

            UnbindTextureShader(renderer);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private record struct ShapeParameters
        {
            public UniformFloat BorderThickness;
            public UniformFloat ShadowRadius;
            public UniformBool Glow;
            public UniformPadding4 _;
        }

        private bool isMatchingUniformBlock(IUniformBuffer<ShapeParameters> uniformBuffer)
        {
            return uniformBuffer.Data == parameters;
        }
    }
}
