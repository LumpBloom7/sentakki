// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableTouchHold : DrawableSentakkiHitObject
    {
        private readonly TouchHoldCircle circle;

        public override bool HandlePositionalInput => true;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        protected override double InitialLifetimeOffset => 500;

        public DrawableTouchHold(TouchHold hitObject)
            : base(hitObject)
        {
            AccentColour.Value = Color4.HotPink;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(120);
            Scale = new Vector2(0f);
            RelativeSizeAxes = Axes.None;
            Alpha = 0;
            AlwaysPresent = true;
            AddRangeInternal(new Drawable[] {
                circle = new TouchHoldCircle(){ Duration = hitObject.Duration },
            });
        }

        private double potential = 0;
        private double held = 0;
        private bool buttonHeld = false;

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current < HitObject.StartTime) return;

            if (userTriggered || Time.Current < (HitObject as IHasEndTime)?.EndTime)
                return;

            double result = held / potential;

            ApplyResult(r =>
            {
                if (result >= .9)
                    r.Type = HitResult.Perfect;
                else if (result >= .8)
                    r.Type = HitResult.Great;
                else if (result >= .5)
                    r.Type = HitResult.Good;
                else if (result >= .2)
                    r.Type = HitResult.Ok;
                else if (Time.Current >= (HitObject as IHasEndTime)?.EndTime)
                    r.Type = HitResult.Miss;
            });
        }

        public bool Auto = false;

        protected override void Update()
        {
            double fadeIn = 500 * (Clock.Rate < 0 ? 1 : Clock.Rate);
            double animStart = HitObject.StartTime - fadeIn;
            double currentProg = Clock.CurrentTime - animStart;

            // Calculate initial entry animation
            float fadeAmount = (float)(currentProg / fadeIn);
            if (fadeAmount < 0) fadeAmount = 0;
            else if (fadeAmount > 1) fadeAmount = 1;

            Alpha = fadeAmount;
            Scale = new Vector2(1f * fadeAmount);

            // Calculate progressbar fill
            float fillAmount = (float)((currentProg - fadeIn) / (HitObject as TouchHold).Duration);
            if (fillAmount < 0) fillAmount = 0;
            else if (fillAmount > 1) fillAmount = 1;

            circle.Progress.Current.Value = fillAmount;

            // Hidden fade calculation
            float hiddenAmount = (float)((currentProg - fadeIn) / 250);
            if (hiddenAmount < 0) hiddenAmount = 0;
            else if (hiddenAmount > 1) hiddenAmount = 1;

            if (IsHidden && hiddenAmount > 0)
                Alpha = 1 - (1 * hiddenAmount);

            // Input and feedback
            buttonHeld = SentakkiActionInputManager?.PressedActions.Any(x => x == SentakkiAction.Button1 || x == SentakkiAction.Button2) ?? false;
            if (Time.Current >= HitObject.StartTime && Time.Current <= (HitObject as IHasEndTime)?.EndTime)
            {
                potential++;
                if ((buttonHeld && IsHovered) || Auto)
                {
                    held++;
                    circle.FadeTo((IsHidden) ? .2f : 1f, 100);
                    circle.ScaleTo(1f, 100);
                    circle.Glow.FadeTo(1f, 100);
                }
                else
                {
                    circle.FadeTo((IsHidden) ? 0 : .5f, 100);
                    circle.ScaleTo(.8f, 200);
                    circle.Glow.FadeTo(0f, 200);
                }
                base.Update();
            }
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);
            const double time_fade_hit = 400, time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    this.Delay((HitObject as IHasEndTime).Duration).ScaleTo(1f, time_fade_hit).Expire();
                    break;

                case ArmedState.Miss:
                    this.Delay((HitObject as IHasEndTime).Duration).ScaleTo(.0f, time_fade_miss).FadeOut(time_fade_miss);
                    break;
            }
        }
    }
}
