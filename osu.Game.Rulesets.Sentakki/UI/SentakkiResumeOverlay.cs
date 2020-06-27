using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Skinning;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class SentakkiResumeOverlay : ResumeOverlay
    {
        protected override string Message => "Prepare for unforeseen consequences...";

        private double timePassed = 500;
        private Bindable<int> tickCount = new Bindable<int>(3);

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
                countSound = new SkinnableSound(new SampleInfo("count"))
            };
            tickCount.BindValueChanged(ticks => counterText.Text = ticks.NewValue.ToString());
        }

        protected override void Update()
        {
            base.Update();
            if (tickCount.Value > 0)
            {
                timePassed += Clock.ElapsedFrameTime;
                if (timePassed > 1000)
                {
                    timePassed %= 1000;
                    --tickCount.Value;
                    if (tickCount.Value > 0)
                        countSound?.Play();
                }
                if (tickCount.Value == 0)
                    Resume();
            }
        }

        protected override void PopIn()
        {
            base.PopIn();
            tickCount.Value = 4;
            counterText.Text = "";
            timePassed = 500;
        }
    }
}