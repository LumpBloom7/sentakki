using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default.Slides
{
    public class SlideTapPiece : CompositeDrawable
    {
        public readonly Container Stars;
        public readonly SkinnableDrawable SecondStar;

        public SlideTapPiece()
        {
            Size = new Vector2(75);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                Stars = new Container(){
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]{
                        new SkinnableDrawable(new SentakkiSkinComponent(SentakkiSkinComponents.SlideStar), _ => new StarPiece())
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.None,
                        },
                        SecondStar = new SkinnableDrawable(new SentakkiSkinComponent(SentakkiSkinComponents.SlideStar), _ => new StarPiece())
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.None,
                            Rotation = 36
                        }
                    }
                },
            };
        }

        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour =>
            {
                Stars.Colour = colour.NewValue;
            }, true);
        }
    }
}
