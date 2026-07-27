using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Skinning.Common;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public partial class LegacyTouchBody : CompositeDrawable, ITouchBody
{
    public LegacyTouchBody()
    {
        RelativeSizeAxes = Axes.Both;
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
    }

    public Drawable Border { get; private set; } = null!;

    private Container colourContainer = null!;
    private Container glowContainer = null!;

    private IBindable<Color4> accentColour = new Bindable<Color4>();
    private IBindable<bool> exBindable = new Bindable<bool>();

    [BackgroundDependencyLoader]
    private void load(DrawableHitObject? drawableHitObject, ISkinSource skin)
    {
        InternalChildren = [
            colourContainer = new Container {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,

                Children = [
                    glowContainer = createTouchGlow(skin),
                    createTouchShape(skin),
                    new LegacyTouchDot(),
                ]
            },

            Border = new Sprite {
                RelativeSizeAxes = Axes.Both,
                Texture = skin.GetTexture("sentakki/touch-border"),
                FillMode = FillMode.Fit,
                Alpha = 0,
            },
        ];

        if (drawableHitObject is not DrawableSentakkiHitObject dsho)
            return;

        accentColour.BindTo(dsho.AccentColour);
        accentColour.BindValueChanged(colour => colourContainer.Colour = colour.NewValue, true);

        exBindable.BindTo(dsho.ExBindable);
        exBindable.BindValueChanged(e => glowContainer.Colour = e.NewValue ? Color4.White : Color4.Black, true);
    }

    private Container createTouchShape(ISkinSource skin)
    {
        var texture = skin.GetTexture("sentakki/touch");

        var container = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
        };

        float[] angles = [0, 180, 270, 90];
        Anchor[] anchors = [Anchor.TopCentre, Anchor.BottomCentre, Anchor.CentreLeft, Anchor.CentreRight];

        for (int i = 0; i < 4; ++i)
        {
            container.Add(new Container
            {
                Size = new Vector2(75, 45),
                Anchor = anchors[i],
                Origin = Anchor.TopCentre,
                Rotation = angles[i],
                Child = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    Texture = texture,
                }
            });
        }

        return container;
    }

    private Container createTouchGlow(ISkinSource skin)
    {
        var texture = skin.GetTexture("sentakki/glow/touch");

        var container = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
        };

        float[] angles = [0, 180, 270, 90];
        Anchor[] anchors = [Anchor.TopCentre, Anchor.BottomCentre, Anchor.CentreLeft, Anchor.CentreRight];

        for (int i = 0; i < 4; ++i)
        {
            container.Add(new Container
            {
                Anchor = anchors[i],
                Rotation = angles[i],
                Size = new Vector2(75, 45),
                Origin = Anchor.TopCentre,

                Child = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(1.5f),
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    Texture = texture,
                }
            });
        }

        return container;
    }
}
