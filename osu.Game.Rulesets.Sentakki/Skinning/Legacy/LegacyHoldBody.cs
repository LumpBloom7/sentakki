using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Skinning.Default;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public partial class LegacyHoldBody : CompositeDrawable
{
    private Container accentContainer = null!;
    private Drawable glowDrawable = null!;

    public LegacyHoldBody()
    {
        RelativeSizeAxes = Axes.Both;
    }

    private readonly IBindable<Color4> accentColour = new Bindable<Color4>(Color4.White);

    private readonly IBindable<bool> exState = new Bindable<bool>();

    private readonly IBindable<bool> isHitting = new Bindable<bool>();

    private Color4 flashingColour = Color4.White;

    [BackgroundDependencyLoader]
    private void load(ISkinSource skin, DrawableHitObject? drawableHitObject)
    {
        InternalChildren =
        [
            accentContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,

                Children =
                [
                    glowDrawable = createGlowLayer(skin),
                    createLayer(skin)
                ]
            },
            createLayer(skin, "_overlay"),
        ];

        if (drawableHitObject is not DrawableHold drawableHold)
            return;

        accentColour.BindTo(drawableHold.AccentColour);
        accentColour.BindValueChanged(c => flashingColour = c.NewValue.LightenHsl(0.4f), true);

        exState.BindTo(drawableHold.ExBindable);
        exState.BindValueChanged(ex => glowDrawable.Colour = ex.NewValue ? Color4.White : Color4.Black, true);

        isHitting.BindTo(drawableHold.IsHitting);
    }

    protected override void Update()
    {
        base.Update();

        if (!isHitting.Value)
        {
            accentContainer.Colour = accentColour.Value;
            return;
        }

        const double flashing_time = 80;

        double flashProg = Time.Current % (flashing_time * 2) / (flashing_time * 2);

        if (flashProg <= 0.5)
            accentContainer.Colour = Interpolation.ValueAt(flashProg, accentColour.Value, flashingColour, 0, 0.5, Easing.OutSine);
        else
            accentContainer.Colour = Interpolation.ValueAt(flashProg, flashingColour, accentColour.Value, 0.5, 0, Easing.InSine);
    }

    private static Container createLayer(ISkinSource skin, string texturePostfix = "")
    {
        var bodyTexture = skin.GetTexture($"sentakki/hitobjects/hold/body{texturePostfix}", WrapMode.Repeat, WrapMode.Repeat);
        bodyTexture = bodyTexture?.Crop(new RectangleF(0, 1, bodyTexture.Width, bodyTexture.Height - 2), Axes.None, WrapMode.ClampToEdge, WrapMode.ClampToEdge);

        var headTexture = skin.GetTexture($"sentakki/hitobjects/hold/head{texturePostfix}");
        var tailTexture = skin.GetTexture($"sentakki/hitobjects/hold/tail{texturePostfix}");

        return new Container
        {
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding { Vertical = TapRing.CIRCLE_RADIUS },

            Children =
            [
                new HoldCap(headTexture)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(TapRing.CIRCLE_RADIUS * 2)
                },
                new HoldCap(tailTexture)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(TapRing.CIRCLE_RADIUS * 2),
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Child = new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Texture = bodyTexture,
                    }
                },
            ]
        };
    }

    private static Container createGlowLayer(ISkinSource skin)
    {
        var bodyTexture = skin.GetTexture("sentakki/hitobjects/hold/body_glow", WrapMode.Repeat, WrapMode.Repeat);
        var headTexture = skin.GetTexture("sentakki/hitobjects/hold/head_glow");
        var tailTexture = skin.GetTexture("sentakki/hitobjects/hold/tail_glow");

        return new Container
        {
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding { Vertical = TapRing.CIRCLE_RADIUS },

            Children =
            [
                new HoldCap(headTexture)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(TapRing.CIRCLE_RADIUS * 2),
                    Scale = new Vector2(1.5f),
                },
                new HoldCap(tailTexture)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(TapRing.CIRCLE_RADIUS * 2),
                    Scale = new Vector2(1.5f),
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Scale = new Vector2(1.5f, 1.0f),
                    Padding = new MarginPadding
                    {
                        Top = -0.3f,
                        Bottom = -0.3f,
                    },
                    Child = new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Texture = bodyTexture,
                    }
                },
            ]
        };
    }

    private partial class HoldCap : CompositeDrawable
    {
        public HoldCap(Texture? texture)
        {
            if (texture is null)
                return;

            var top = texture.Crop(
                new RectangleF(
                    Vector2.Zero,
                    new Vector2(texture.Width, MathF.Floor((float)texture.Height / 2) - 1)),
                Axes.None, WrapMode.ClampToEdge, WrapMode.ClampToEdge);


            var bottom = texture.Crop(
                new RectangleF(
                    new Vector2(0, (texture.Size.Y / 2 + 1)),
                    new Vector2(texture.Width, MathF.Floor((float)texture.Height / 2) - 1)),
                Axes.None, WrapMode.ClampToEdge, WrapMode.ClampToEdge);

            InternalChildren =
            [
                new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    Texture = top,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1, 0.5f)
                },
                new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    Texture = bottom,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1, 0.5f)
                },
            ];
        }
    }
}
