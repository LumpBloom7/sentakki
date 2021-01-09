using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class SentakkiResumeOverlay : ResumeOverlay
    {
        private static readonly string[] supporter_list = new string[]{
            "Ayato_K ♥",
            "Bosch ♥",
            "Dubita ♥",
            "Ezz ♥ (iOS helper)",
            "Mae ♥♥",
            "Nutchapol ♥",
            "Shiuannie (Artist)",
            "Smoogipoo ♥♥♥",
            "Flutterish (Debate starter)",
            "Slipsy (Discord moderator)",
            "Nooraldeen (Discord moderator, feedback machine)",
            "lazer developers"
        }.OrderBy(t => RNG.Next()).ToArray();

        private static int currentSupporterIndex;

        protected override string Message => "Get ready!";

        private double timePassed = 3500;
        private Bindable<int> tickCount = new Bindable<int>(4);

        private OsuSpriteText counterText;
        private OsuSpriteText supporterText;

        private readonly SkinnableSound countSound;

        private SentakkiCursorContainer localCursorContainer;

        public override CursorContainer LocalCursor => State.Value == Visibility.Visible ? localCursorContainer : null;

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
                    Shadow = true,
                    ShadowColour = new Color4(0f, 0f, 0f, 0.25f)
                },
                new FillFlowContainer{
                    Direction = FillDirection.Horizontal,
                    RelativePositionAxes = Axes.Both,
                    Y = -0.4f,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Children = new Drawable[]{
                        new OsuSpriteText
                        {
                            Text = "This ruleset is made possible with the support of ",
                            Font = OsuFont.Torus.With(size: 20),
                            Colour = Color4.White,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Shadow = true,
                            ShadowColour = new Color4(0f, 0f, 0f, 0.25f)
                        },
                        supporterText = new OsuSpriteText
                        {
                            Text = "Marisa Kirisame",
                            Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold),
                            Colour = Color4.HotPink,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Shadow = true,
                            ShadowColour = new Color4(0f, 0f, 0f, 0.25f)
                        }
                    }
                },
                countSound = new SkinnableSound(new SampleInfo("Gameplay/Taka"))
            };
            tickCount.BindValueChanged(
                ticks =>
                {
                    counterText.Text = (ticks.NewValue == 4) ? "" : ticks.NewValue.ToString();
                    if (ticks.NewValue % 4 != 0)
                        countSound?.Play();
                    if (ticks.NewValue <= 0) Resume();
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
            supporterText.Text = getRandomSupporter();

            // Reset the countdown
            timePassed = 3500;

            GameplayCursor.ActiveCursor.Hide();

            if (localCursorContainer == null)
            {
                Add(localCursorContainer = new SentakkiCursorContainer());
                localCursorContainer.MoveTo(GameplayCursor.ActiveCursor.Position);
            }
        }

        protected override void PopOut()
        {
            base.PopOut();

            if (localCursorContainer != null && GameplayCursor?.ActiveCursor != null)
                GameplayCursor.ActiveCursor.Position = localCursorContainer.Position;

            localCursorContainer?.Expire();
            localCursorContainer = null;
            GameplayCursor?.ActiveCursor?.Show();
        }

        private string getRandomSupporter()
        {
            var tmp = supporter_list[currentSupporterIndex++];
            if (currentSupporterIndex >= supporter_list.Length) currentSupporterIndex = 0;

            return tmp;
        }
    }
}
