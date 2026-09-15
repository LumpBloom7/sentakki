using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

    private readonly IBindable<Color4> accentColour = new Bindable<Color4>();
    private readonly IBindable<bool> exState = new Bindable<bool>();

    private readonly IBindable<bool> isHitting = new Bindable<bool>();

    private Color4 flashingColour = Color4.White;


    [BackgroundDependencyLoader]
    private void load(ISkinSource skin, DrawableHitObject? drawableHitObject)
    {
        InternalChildren = [
            accentContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,

                Children = [
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

    private static GridContainer createLayer(ISkinSource skin, string texturePostfix = "")
    {
        var bodyTexture = skin.GetTexture($"sentakki/hitobjects/hold/body{texturePostfix}", WrapMode.ClampToEdge, WrapMode.ClampToEdge);
        var headTexture = skin.GetTexture($"sentakki/hitobjects/hold/head{texturePostfix}", WrapMode.ClampToEdge, WrapMode.ClampToEdge);
        var tailTexture = skin.GetTexture($"sentakki/hitobjects/hold/tail{texturePostfix}", WrapMode.ClampToEdge, WrapMode.ClampToEdge);

        return new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            RowDimensions = [
                new Dimension(GridSizeMode.Absolute, TapRing.CIRCLE_RADIUS),
                new Dimension(GridSizeMode.Distributed),
                new Dimension(GridSizeMode.Absolute, TapRing.CIRCLE_RADIUS)
            ],

            Content = new Drawable[][]
            {
                [
                    new Sprite
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Height = 2,

                        Texture = headTexture,
                    },
                ],

                [
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Texture = bodyTexture,
                        }
                    }
                ],

                [
                    new Sprite
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Height = 2,

                        Texture = tailTexture,
                    }
                ]
            }
        };
    }

    private static GridContainer createGlowLayer(ISkinSource skin)
    {
        var bodyTexture = skin.GetTexture("sentakki/hitobjects/hold/body_glow", WrapMode.ClampToEdge, WrapMode.ClampToEdge);
        var headTexture = skin.GetTexture("sentakki/hitobjects/hold/head_glow", WrapMode.ClampToEdge, WrapMode.ClampToEdge);
        var tailTexture = skin.GetTexture("sentakki/hitobjects/hold/tail_glow", WrapMode.ClampToEdge, WrapMode.ClampToEdge);

        return new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            RowDimensions = [
                new Dimension(GridSizeMode.Absolute, TapRing.CIRCLE_RADIUS),
                new Dimension(GridSizeMode.Distributed),
                new Dimension(GridSizeMode.Absolute, TapRing.CIRCLE_RADIUS)
            ],

            Content = new Drawable[][]
            {
                [
                    new Sprite
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Height = 2,
                        Scale = new Vector2(1.5f),

                        Texture = headTexture,
                    },
                ],

                [
                    new Sprite
                    {
                        Scale = new Vector2(1.5f, 1.0f),
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Texture = bodyTexture,
                    }
                ],

                [
                    new Sprite
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Height = 2,
                        Scale = new Vector2(1.5f),

                        Texture = tailTexture,
                    }
                ]
            }
        };
    }
}
