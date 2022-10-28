using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Skinning.Default
{
    public class HoldBody : CompositeDrawable
    {
        public HoldBody()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.BottomCentre;
            RelativeSizeAxes = Axes.Both;
            InternalChildren = new Drawable[]
            {
                new NoteRingPiece(),
                new DotPiece(squared: true)
                {
                    Rotation = 45,
                    Anchor = Anchor.BottomCentre,
                },
                new DotPiece(squared: true)
                {
                    Rotation = 45,
                    Anchor = Anchor.TopCentre,
                },
            };
        }

        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour => Colour = colour.NewValue, true);
        }
    }
}
