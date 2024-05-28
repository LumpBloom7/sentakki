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
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;

public partial class RingNote : Sprite, ITexturedShaderDrawable
{
    private bool hex;
    public bool Hex
    {
        get => hex;
        set
        {
            if (hex == value)
                return;
            hex = value;
            Invalidate(Invalidation.DrawNode);
        }
    }

    private float thickness = 0.25f;
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

    private float shadowRadius = 15f / 105f;
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

    private Color4 secondaryColor = Color4.Gray;
    public Color4 SecondaryColor
    {
        get => secondaryColor;
        set
        {
            if (secondaryColor == value)
                return;
            secondaryColor = value;
            Invalidate(Invalidation.DrawNode);
        }
    }

    public new IShader TextureShader { get; private set; } = null!;

    protected override DrawNode CreateDrawNode() => new RingNoteDrawNode(this);

    private BindableBool exBindable = new BindableBool();

    [BackgroundDependencyLoader]
    private void load(ShaderManager shaders, IRenderer renderer, DrawableHitObject? hitObject)
    {
        TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "ringShader");
        Texture = renderer.WhitePixel;

        if (hitObject is null)
            return;

        // Bind exnote
        exBindable.BindTo(((DrawableSentakkiHitObject)hitObject).ExBindable);
        exBindable.BindValueChanged(b => Glow = b.NewValue, true);
    }

    private partial class RingNoteDrawNode : SpriteDrawNode
    {
        protected new RingNote Source => (RingNote)base.Source;
        protected override bool CanDrawOpaqueInterior => false;
        private IUniformBuffer<ShapeParameters>? shapeParameters;

        private bool hex;
        private float thickness;
        private Vector2 size;
        private bool glow;
        private float shadowRadius;

        public RingNoteDrawNode(RingNote source)
            : base(source)
        {
        }

        public override void ApplyState()
        {
            base.ApplyState();
            hex = Source.Hex;
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
                Hex = hex,
                Thickness = thickness,
                Size = size,
                ShadowRadius = shadowRadius,
                Glow = glow
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
            public UniformBool Hex;
            public UniformFloat Thickness;
            public UniformVector2 Size;
            public UniformFloat ShadowRadius;
            public UniformBool Glow;
            public UniformPadding8 padding_;
        }

    }
}
