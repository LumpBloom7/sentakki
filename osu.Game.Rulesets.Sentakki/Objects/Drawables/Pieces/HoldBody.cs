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
    public partial class HoldBody : CompositeDrawable
    {
        // This will be proxied, so a must.
        public override bool RemoveWhenNotAlive => false;
        public BindableBool IsHitting = new();

        private HitExplosion hitExplosion;
        private Container colourContainer;

        public HoldBody()
        {
            Scale = Vector2.Zero;
            Position = new Vector2(0, -SentakkiPlayfield.NOTESTARTDISTANCE);
            Anchor = Anchor.Centre;
            Origin = Anchor.TopCentre;
            InternalChildren =
            [
                colourContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children =
                    [
                        new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            Alpha = 0.25f,
                            Child = hitExplosion = new HitExplosion
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.Centre,
                                Alpha = 0,
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Child =  new NoteRingPiece(true)
                        },
                    ]
                }
            ];
        }

        private Color4 flashingColor = Color4.White;

        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            accentColour.BindTo(drawableObject.AccentColour);
            accentColour.BindValueChanged(colour =>
            {
                colourContainer.Colour = colour.NewValue;
                flashingColor = colour.NewValue.LightenHSL(0.4f);
            }, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            IsHitting.BindValueChanged(hitting =>
            {
                const float animation_length = 80;

                colourContainer.ClearTransforms();
                hitExplosion.ClearTransforms();

                if (hitting.NewValue)
                {
                    // wait for the next sync point
                    double synchronisedOffset = animation_length * 2 - Time.Current % (animation_length * 2);

                    using (BeginDelayedSequence(synchronisedOffset))
                    {
                        colourContainer.FadeColour(flashingColor, animation_length, Easing.OutSine).Then()
                            .FadeColour(accentColour.Value, animation_length, Easing.InSine)
                            .Loop();

                        hitExplosion.Explode(160).Loop();
                    }
                }
                else
                {
                    colourContainer.FadeColour(accentColour.Value);
                    hitExplosion.Alpha = 0;
                }
            }, true);
        }
    }
}
