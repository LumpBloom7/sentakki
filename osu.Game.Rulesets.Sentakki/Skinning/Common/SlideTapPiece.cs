using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Skinning.Default;
using osu.Game.Rulesets.Sentakki.Skinning.Default.Slide;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Common;

public partial class SlideTapPiece : CompositeDrawable
{
    // This will be proxied, so a must.
    public override bool RemoveWhenNotAlive => false;

    public readonly Container Stars;
    public readonly SkinnableDrawable SecondStar;

    public SlideTapPiece()
    {
        Size = new Vector2(DrawableTap.CIRCLE_RADIUS * 2);

        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        InternalChildren =
        [
            Stars = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children =
                [
                    new SkinnableDrawable(new SentakkiSkinComponentLookup(SentakkiSkinComponents.SlideStar), _=> new StarPiece(), ConfineMode.ScaleToFit)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    SecondStar = new SkinnableDrawable(new SentakkiSkinComponentLookup(SentakkiSkinComponents.SlideStar), _=> new StarPiece(), ConfineMode.ScaleToFit)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Rotation = 36
                    }
                ]
            }
        ];
    }

    private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

    [BackgroundDependencyLoader]
    private void load(DrawableHitObject? drawableObject)
    {
        if (drawableObject is null)
            return;

        accentColour.BindTo(drawableObject.AccentColour);
        accentColour.BindValueChanged(colour =>
        {
            Stars.Colour = colour.NewValue;
        }, true);
    }
}
