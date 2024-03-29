using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Sentakki.Localisation;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.UI
{
    public partial class SentakkiResumeOverlay : ResumeOverlay
    {
        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private DrawableSentakkiRuleset? drawableSentakkiRuleset { get; set; }

        // We don't want the default message
        protected override LocalisableString Message => "";

        private OsuSpriteText messageText = null!;

        private double beatlength;

        private double remainingTime = 3500;

        private readonly Bindable<int> beatsLeft = new Bindable<int>(4);
        private int barLength;

        private SkinnableSound countSound = null!;

        private SentakkiCursorContainer? localCursorContainer;

        public override CursorContainer? LocalCursor => State.Value == Visibility.Visible ? localCursorContainer : null;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                messageText = new OsuSpriteText
                {
                    Font = OsuFont.Torus.With(size: 50, weight: FontWeight.SemiBold),
                    Colour = colours.Yellow,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    ShadowColour = new Color4(0f, 0f, 0f, 0.25f)
                },
                countSound = new SkinnableSound(new SampleInfo("Gameplay/Taka"))
            };

            beatsLeft.ValueChanged += onCountUpdated;
        }

        protected override void Update()
        {
            base.Update();
            if (State.Value == Visibility.Hidden) return;

            remainingTime -= Clock.ElapsedFrameTime;
            beatsLeft.Value = (int)Math.Ceiling(remainingTime / beatlength);
        }

        protected override void PopIn()
        {
            base.PopIn();

            messageText.Text = SentakkiResumeOverlayStrings.GetReady;

            var currentTimingPoint = beatmap.Value.Beatmap.ControlPointInfo.TimingPointAt(beatmap.Value.Track.CurrentTime);
            barLength = currentTimingPoint.TimeSignature.Numerator;
            beatlength = currentTimingPoint.BeatLength / (drawableSentakkiRuleset?.GameplaySpeed ?? 1);

            // Reset the countdown, plus a second for preparation
            remainingTime = (barLength * beatlength) + 1000;

            if (localCursorContainer == null)
                Add(localCursorContainer = new SentakkiCursorContainer());

            localCursorContainer.State.BindTo(GameplayCursor.State);
            GameplayCursor.ActiveCursor.Hide();
        }

        protected override void PopOut()
        {
            base.PopOut();
            messageText.Text = SentakkiResumeOverlayStrings.LetsGo;

            if (localCursorContainer != null && GameplayCursor?.ActiveCursor != null)
            {
                GameplayCursor.ActiveCursor.Position = localCursorContainer.ActiveCursor.Position;
            }

            localCursorContainer?.Expire();
            localCursorContainer = null;
            GameplayCursor?.ActiveCursor?.Show();
        }

        private void onCountUpdated(ValueChangedEvent<int> beatsLeft)
        {
            if (beatsLeft.NewValue < barLength && beatsLeft.NewValue < beatsLeft.OldValue)
            {
                countSound?.Play();

                messageText.Text = beatsLeft.NewValue.ToString();
                messageText.FinishTransforms();
                messageText.ScaleTo(1.1f, 100).Then().ScaleTo(1, 50);
            }

            if (beatsLeft.NewValue <= 0)
                Resume();
        }
    }
}
