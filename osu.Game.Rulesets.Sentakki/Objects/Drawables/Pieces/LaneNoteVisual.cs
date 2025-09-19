using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;

public enum NoteShape
{
    Ring,
    Hex,
    Star
}

public partial class LaneNoteVisual : Sprite, ITexturedShaderDrawable
{
    protected override Quad ComputeConservativeScreenSpaceDrawQuad() => ToScreenSpace(DrawRectangle.Shrink(shadowRadius * 1.5f));

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
        private IUniformBuffer<ShapeParameters>? shapeParameters;

        private float thickness;
        private Vector2 size;
        private bool glow;
        private float shadowRadius;

        public LaneNoteVisualDrawNode(LaneNoteVisual source)
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
        }

        protected override void BindUniformResources(IShader shader, IRenderer renderer)
        {
            base.BindUniformResources(shader, renderer);

            shapeParameters ??= renderer.CreateUniformBuffer<ShapeParameters>();

            shapeParameters.Data = shapeParameters.Data with
            {
                BorderThickness = thickness,
                Size = size,
                ShadowRadius = shadowRadius,
                Glow = glow,
            };

            shader.BindUniformBlock("m_shapeParameters", shapeParameters);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            shapeParameters?.Dispose();
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
    }
}
