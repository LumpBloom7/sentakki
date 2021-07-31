using System;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public class SentakkiResumeOverlay : ResumeOverlay
    {
        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        private readonly string[] supporter_list = new string[]{
            "Ayato_K ♥",
            "Bosch ♥",
            "Dubita ♥",
            "Ezz ♥ (iOS helper)",
            "Mae ♥♥",
            "Nutchapol ♥",
            "Shiuannie (Artist)",
            "Smoogipoo ♥♥♥",
            "Flutterish",
            "Slipsy (Discord moderator)",
            "Nooraldeen (Discord moderator, feedback machine)",
            "lazer developers"
        }.OrderBy(t => RNG.Next()).ToArray();

        private static int currentSupporterIndex;

        protected override string Message => "";

        private TimingControlPoint currentTimingPoint => beatmap.Value.Beatmap.ControlPointInfo.TimingPointAt(beatmap.Value.Track.CurrentTime);

        private int maxTicks => (int)currentTimingPoint.TimeSignature;
        private double beatlength => currentTimingPoint.BeatLength;

        private double timePassed = 3500;
        private Bindable<int> tickCount = new Bindable<int>(4);
        private OsuSpriteText supporterText;

        private SkinnableSound countSound;

        private SentakkiCursorContainer localCursorContainer;

        public override CursorContainer LocalCursor => State.Value == Visibility.Visible ? localCursorContainer : null;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]{
                messageText = new OsuSpriteText
                {
                    Font = OsuFont.Torus.With(size: 50, weight: FontWeight.SemiBold),
                    Colour = colours.Yellow,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
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
                            Text = "Sentakki is made with the support of ",
                            Font = OsuFont.Torus.With(size: 15),
                            Colour = Color4.White,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Shadow = true,
                            ShadowColour = new Color4(0f, 0f, 0f, 0.25f)
                        },
                        supporterText = new OsuSpriteText
                        {
                            Text = "Marisa Kirisame",
                            Font = OsuFont.Torus.With(size: 18, weight: FontWeight.SemiBold),
                            Colour = Color4Extensions.FromHex("ff0064"),
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
                    if (ticks.NewValue < ticks.OldValue)
                    {
                        messageText.Text = ticks.NewValue.ToString();
                        countSound?.Play();
                        messageText.FinishTransforms();
                        messageText.ScaleTo(1.1f, 100).Then().ScaleTo(1, 50);
                    }

                    if (ticks.NewValue <= 0) Resume();
                }
            );
        }

        private OsuSpriteText messageText;

        protected override void LoadComplete()
        {
            base.LoadComplete();
        }

        protected override void Update()
        {
            base.Update();
            if (State.Value == Visibility.Hidden) return;

            timePassed -= Clock.ElapsedFrameTime;
            tickCount.Value = (int)Math.Ceiling(timePassed / beatlength);
        }

        protected override void PopIn()
        {
            base.PopIn();
            supporterText.Text = getRandomSupporter();
            messageText.Text = "Get ready!";

            // Reset the countdown
            timePassed = maxTicks * beatlength;

            GameplayCursor.ActiveCursor.Hide();

            if (localCursorContainer == null)
            {
                Add(localCursorContainer = new SentakkiCursorContainer());
            }
        }

        protected override void PopOut()
        {
            base.PopOut();
            messageText.Text = "Let's go!";

            if (localCursorContainer != null && GameplayCursor?.ActiveCursor != null)
                GameplayCursor.ActiveCursor.Position = localCursorContainer.ActiveCursor.Position;

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
