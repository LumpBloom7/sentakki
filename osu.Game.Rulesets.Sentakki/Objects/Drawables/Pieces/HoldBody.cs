using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class HoldBody : CompositeDrawable
    {
        // This will be proxied, so a must.
        public override bool RemoveWhenNotAlive => false;

        public HoldBody()
        {
            Scale = Vector2.Zero;
            Position = new Vector2(0, -SentakkiPlayfield.NOTESTARTDISTANCE);
            Anchor = Anchor.Centre;
            Origin = Anchor.BottomCentre;
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
