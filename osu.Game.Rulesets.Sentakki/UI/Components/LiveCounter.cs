using System;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI.Components
{
    public class LiveCounter : BeatSyncedContainer
    {
        public BindableInt LivesLeft = new BindableInt(300);

        private OsuSpriteText livesText;

        public LiveCounter()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]{

                livesText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Torus.With(size: 40, weight: FontWeight.SemiBold),
                    ShadowColour = Color4.Gray,
                },
            };

            LivesLeft.BindValueChanged(v =>
            {
                livesText.Text = v.NewValue.ToString();
                this.FadeColour(Color4.Red, 160).Then().FadeColour(Color4.White, 320);
                Shake();
            }, true);
        }

        public void Shake(double? maximumLength = null)
        {
            const float shake_amount = 8;
            const double shake_duration = 80;

            // if we don't have enough time, don't bother shaking.
            if (maximumLength < shake_duration * 2)
                return;

            var sequence = this.MoveToX(shake_amount, shake_duration / 2, Easing.OutSine).Then()
                               .MoveToX(-shake_amount, shake_duration, Easing.InOutSine).Then();

            // if we don't have enough time for the second shake, skip it.
            if (!maximumLength.HasValue || maximumLength >= shake_duration * 4)
            {
                sequence = sequence
                           .MoveToX(shake_amount, shake_duration, Easing.InOutSine).Then()
                           .MoveToX(-shake_amount, shake_duration, Easing.InOutSine).Then();
            }

            sequence.MoveToX(0, shake_duration / 2, Easing.InSine);
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            if (beatIndex % (int)timingPoint.TimeSignature == 0)
                this.ScaleTo(1.1f, 200).Then().ScaleTo(1, 120).Then().ScaleTo(1.1f, 160).Then().ScaleTo(1, 320);
        }
    }
}
