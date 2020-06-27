using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Skinning;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play;
using osuTK.Graphics;
using System;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class SentakkiResumeOverlay : ResumeOverlay
    {
        protected override string Message => "Prepare for unforeseen consequences...";

        private double timePassed = 3500;
        private Bindable<int> tickCount = new Bindable<int>(4);

        private OsuSpriteText counterText;
        private readonly SkinnableSound countSound;

        public SentakkiResumeOverlay()
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fill;
            Children = new Drawable[]{
                counterText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "",
                    Font = OsuFont.Torus.With(size: 50),
                    Colour = Color4.White,
                },
                countSound = new SkinnableSound(new SampleInfo("Taka"))
            };
            tickCount.BindValueChanged(
                ticks =>
                {
                    counterText.Text = (ticks.NewValue == 4) ? "" : ticks.NewValue.ToString();
                    if (ticks.NewValue % 4 != 0) countSound?.Play();
                    if (ticks.NewValue == 0) Resume();
                }
            );
        }

        protected override void Update()
        {
            base.Update();
            timePassed -= Clock.ElapsedFrameTime;
            tickCount.Value = (int)Math.Ceiling(timePassed / 1000);
        }

        protected override void PopIn()
        {
            base.PopIn();

            // Reset the countdown
            timePassed = 3500;
        }
    }
}