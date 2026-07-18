using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Legacy;

public partial class LegacyStarPiece : CompositeDrawable
{
    private Sprite glowLayer = null!;

    public LegacyStarPiece()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    private IBindable<bool> exBindable = new Bindable<bool>();

    [BackgroundDependencyLoader]
    private void load(DrawableHitObject? drawableHitObject, ISkinSource skin)
    {
        AddRangeInternal([
            glowLayer = new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(1.5f),

                Texture = skin.GetTexture("sentakki/glow/star"),
            },
            new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                Texture = skin.GetTexture("sentakki/star")
            }
        ]);

        if (drawableHitObject is not DrawableSentakkiHitObject dsho)
            return;

        exBindable.BindTo(dsho.ExBindable);
        exBindable.BindValueChanged(e => glowLayer.Colour = e.NewValue ? Color4.White : Color4.Black, true);
    }
}
