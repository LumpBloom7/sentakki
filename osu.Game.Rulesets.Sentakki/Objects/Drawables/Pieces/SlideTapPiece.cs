using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class SlideTapPiece : CompositeDrawable
    {
        // This will be proxied, so a must.
        public override bool RemoveWhenNotAlive => false;

        public readonly Container Stars;

        private readonly ExplodePiece explode;

        public SlideTapPiece()
        {
            Size = new Vector2(80);

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
                    Child = new StarPiece()
                },
                explode = new ExplodePiece(),
            };
        }

        private readonly IBindable<ArmedState> state = new Bindable<ArmedState>();
        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject, DrawableSlide slideObject)
        {
            Tap osuObject = (Tap)drawableObject.HitObject;

            if (slideObject.HitObject.NestedHitObjects.Count > 2) // One is the tap, the others are slidebodies, which we are using
                Stars.Add(new StarPiece { Rotation = 36 });

            state.BindTo(drawableObject.State);
            state.BindValueChanged(updateState, true);

            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour =>
            {
                explode.Colour = colour.NewValue;
                Stars.Colour = colour.NewValue;
            }, true);
        }

        private void updateState(ValueChangedEvent<ArmedState> state)
        {
            switch (state.NewValue)
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
