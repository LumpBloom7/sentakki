using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class TapPiece : CompositeDrawable
    {
        // This will be proxied, so a must.
        public override bool RemoveWhenNotAlive => false;

        private readonly CirclePiece circle;
        private readonly HitExplosion explode;
        private readonly ShadowPiece glow;

        public TapPiece()
        {
            Size = new Vector2(75);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = new Vector2(0f);
            Position = new Vector2(0, -SentakkiPlayfield.NOTESTARTDISTANCE);

            InternalChildren = new Drawable[]
            {
                glow = new ShadowPiece(),
                circle = new CirclePiece(),
                explode = new HitExplosion(),
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
                circle.Colour = colour.NewValue;
            }, true);

            drawableObject.ApplyCustomUpdateState += updateState;
        }

        private void updateState(DrawableHitObject drawableObject, ArmedState state)
        {
            if (!(drawableObject is DrawableTap)) return;
            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime, true))
            {
                switch (state)
                {
                    case ArmedState.Hit:
                        explode.Animate();

                        //after the flash, we can hide some elements that were behind it
                        circle.FadeOut();
                        glow.FadeOut();

                        this.ScaleTo(1.5f, 400, Easing.OutQuad).FadeOut(800);
                        break;
                }
            }
        }
    }
}
