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
    public class SlideTapPiece : CompositeDrawable
    {
        // This will be proxied, so a must.
        public override bool RemoveWhenNotAlive => false;

        public readonly Container Stars;
        public readonly StarPiece SecondStar;

        private readonly ExplodePiece explode;

        public SlideTapPiece()
        {
            Size = new Vector2(75);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = new Vector2(0f);
            Position = new Vector2(0, -SentakkiPlayfield.NOTESTARTDISTANCE);

            InternalChildren = new Drawable[]
            {
                Stars = new Container(){
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]{
                        new StarPiece(),
                        SecondStar = new StarPiece { Rotation = 36 }
                    }
                },
                explode = new ExplodePiece(),
            };
        }

        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour =>
            {
                explode.Colour = colour.NewValue;
                Stars.Colour = colour.NewValue;
            }, true);

            drawableObject.ApplyCustomUpdateState += updateState;
        }

        private void updateState(DrawableHitObject drawableObject, ArmedState state)
        {
            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime, true))
            {
                switch (state)
                {
                    case ArmedState.Hit:
                        const double flash_in = 40;

                        explode.FadeIn(flash_in);
                        this.ScaleTo(1.5f, 400, Easing.OutQuad);

                        using (BeginDelayedSequence(flash_in, true))
                        {
                            Stars.FadeOut();
                            this.FadeOut(800);
                        }

                        break;
                }
            }
        }
    }
}
