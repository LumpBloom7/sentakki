using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.UI.Components;
using osuTK;
using osuTK.Audio.OpenAL;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds
{
    public partial class TouchHoldBody : CircularContainer
    {
        public readonly TouchHoldProgressPiece ProgressPiece;
        public readonly TouchHoldCentrePiece centrePiece;
        public readonly TouchHoldCompletedCentre CompletedCentre;

        private readonly HitExplosion hitExplosion;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => ProgressPiece.ReceivePositionalInputAt(screenSpacePos);

        public TouchHoldBody()
        {
            Size = new Vector2(130);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            InternalChildren = new Drawable[]
            {
                new Container{
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0.25f,
                    Child = hitExplosion = new HitExplosion(true)
                    {
                        Size = new Vector2(80),
                        Rotation = 45f
                    },
                },
                ProgressPiece = new TouchHoldProgressPiece(),
                centrePiece = new TouchHoldCentrePiece(),
                // We swap the centre piece with this other drawable to make it look better with the progress bar
                // Otherwise we'd need to add a thick border in between the centre and the progress
                CompletedCentre = new TouchHoldCompletedCentre(),
                new DotPiece(),
            };
        }

        [Resolved]
        private Bindable<bool> isHitting { get; set; } = null!;

        [Resolved]
        private DrawableTouchHold drawableTouchHold { get; set; } = null!;

        private Bindable<Color4> accentColour = new();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            accentColour.BindTo(drawableTouchHold.AccentColour);

            isHitting.BindValueChanged(hitting =>
            {
                const float animation_length = 80;

                hitExplosion.ClearTransforms();

                if (hitting.NewValue)
                {
                    // wait for the next sync point
                    double synchronisedOffset = animation_length * 2 - Time.Current % (animation_length * 2);

                    using (BeginDelayedSequence(synchronisedOffset))
                    {
                        hitExplosion.Explode(160).Loop();
                    }
                }
                else
                {
                    hitExplosion.Alpha = 0;
                }
            }, true);

            accentColour.BindValueChanged(c => hitExplosion.Colour = c.NewValue);
        }
    }
}
