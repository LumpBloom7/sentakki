using System;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI.Components
{
    public class LiveCounter : BeatSyncedContainer
    {
        public BindableInt LivesLeft = new BindableInt();

        private LiveRollingCounter livesText;

        public LiveCounter(BindableInt livesBindable)
        {
            LivesLeft.BindTo(livesBindable);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChild = livesText = new LiveRollingCounter();

            LivesLeft.BindValueChanged(v =>
            {
                this.FadeColour(Color4.Red, 160).Then().FadeColour(Color4.White, 320);
                Shake();
            }, true);
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();
            livesText.Current.BindTo(LivesLeft);
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
            float livesPercentage = LivesLeft.Value / (float)LivesLeft.MaxValue;

            int panicLevel = 0;
            if (livesPercentage <= 0.1f) panicLevel = 3;
            else if (livesPercentage <= 0.25f) panicLevel = 2;
            else if (livesPercentage <= 0.5f) panicLevel = 1;

            // Heart Rate increases when panicking (lives <= 20% MaxLives)
            // It also beats harder
            float panicDurationMultiplier = 1 * MathF.Pow(0.75f, panicLevel);
            float beatMagnitude = 0.1f + (0.05f * panicLevel);

            if (beatIndex % (int)(timingPoint.TimeSignature.Numerator * Math.Max(panicDurationMultiplier, 0.5f)) == 0)
                this.ScaleTo(1 + beatMagnitude, 200 * panicDurationMultiplier)
                    .Then().ScaleTo(1, 120 * panicDurationMultiplier)
                    .Then().ScaleTo(1 + beatMagnitude, 160 * panicDurationMultiplier)
                    .Then().ScaleTo(1, 320 * panicDurationMultiplier);
        }

        private class LiveRollingCounter : RollingCounter<int>
        {
            protected override double RollingDuration => 1000;

            public LiveRollingCounter()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            protected override OsuSpriteText CreateSpriteText()
            {
                return new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Torus.With(size: 40, weight: FontWeight.SemiBold),
                    ShadowColour = Color4.Gray,
                };
            }
        }
    }
}
