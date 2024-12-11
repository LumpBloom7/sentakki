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

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;

public partial class DrawableChevron : Sprite, ITexturedShaderDrawable
{
    public NoteShape Shape { get; init; } = NoteShape.Ring;
    private float thickness = 7f;
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

    private BindableBool exBindable = new BindableBool();

    [BackgroundDependencyLoader]
    private void load(ShaderManager shaders, IRenderer renderer, DrawableHitObject? hitObject)
    {
        TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "chevron");
        Texture = renderer.WhitePixel;

        if (hitObject is null)
            return;

        // Bind exnote
        exBindable.BindTo(((DrawableSentakkiHitObject)hitObject).ExBindable);
        exBindable.BindValueChanged(b => Glow = b.NewValue, true);
    }

    private partial class ChevronDrawNode : SpriteDrawNode
    {
        protected new DrawableChevron Source => (DrawableChevron)base.Source;
        protected override bool CanDrawOpaqueInterior => false;
        private IUniformBuffer<ShapeParameters>? shapeParameters;

        private float thickness;
        private Vector2 size;
        private bool glow;
        private float shadowRadius;
        private bool fanChev;

        public ChevronDrawNode(DrawableChevron source)
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
            fanChev = Source.FanChevron;
        }

        protected override void BindUniformResources(IShader shader, IRenderer renderer)
        {
            base.BindUniformResources(shader, renderer);

            shapeParameters ??= renderer.CreateUniformBuffer<ShapeParameters>();

            shapeParameters.Data = shapeParameters.Data with
            {
                Thickness = thickness,
                Size = size,
                ShadowRadius = shadowRadius,
                Glow = glow,
                FanChevron = fanChev
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
            public UniformFloat Thickness;
            public UniformPadding4 _;
            public UniformVector2 Size;
            public UniformFloat ShadowRadius;
            public UniformBool Glow;
            public UniformBool FanChevron;

            public UniformPadding4 __;
        }
    }
}
