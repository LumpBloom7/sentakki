using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Touches;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableTouch : DrawableSentakkiHitObject
    {
        protected new Touch HitObject => (Touch)base.HitObject;

        public TouchBody TouchBody = null!;

        private SentakkiInputManager sentakkiActionInputManager = null!;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= (SentakkiInputManager)GetContainingInputManager();

        public DrawableTouch()
            : this(null)
        {
        }

        public DrawableTouch(Touch? hitObject)
            : base(hitObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (DrawableSentakkiRuleset is not null)
                AnimationDuration.BindTo(DrawableSentakkiRuleset?.AdjustedTouchAnimDuration);

            Size = new Vector2(130);
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            AddRangeInternal(new Drawable[]
            {
                TouchBody = new TouchBody(),
            });
        }

        protected override void OnApply()
        {
            base.OnApply();
            Position = HitObject.Position;
        }

        private int pressedCount = 0;

        protected override void Update()
        {
            base.Update();

            int updatedPressedCounts = countActiveTouchPoints();

            if (updatedPressedCounts > pressedCount)
                UpdateResult(true);

            pressedCount = updatedPressedCounts;
        }

        private int countActiveTouchPoints()
        {
            var touchInput = SentakkiActionInputManager.CurrentState.Touch;
            int count = 0;

            if (ReceivePositionalInputAt(SentakkiActionInputManager.CurrentState.Mouse.Position))
            {
                foreach (var item in SentakkiActionInputManager.PressedActions)
                {
                    if (item < SentakkiAction.Key1)
                        ++count;
                }
            }

            foreach (TouchSource source in touchInput.ActiveSources)
            {
                if (touchInput.GetTouchPosition(source) is Vector2 touchPosition && ReceivePositionalInputAt(touchPosition))
                    ++count;
            }

            return count;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            double animTime = AnimationDuration.Value * 0.8;
            double fadeTime = AnimationDuration.Value * 0.2;

            TouchBody.FadeIn(fadeTime);

            using (BeginDelayedSequence(fadeTime))
            {
                TouchBody.ResizeTo(90, animTime, Easing.InCirc);
                TouchBody.BorderContainer.Delay(animTime).FadeIn();
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            // Don't allow for user input if auto is enabled for touch based objects (AutoTouch mod)
            if (!userTriggered || Auto)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(Result.Judgement.MinResult);
                else if (Auto && timeOffset > 0) // Hack: this is chosen to be "strictly larger" so that it remains visible
                    ApplyResult(Result.Judgement.MaxResult);

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);

            // This is hit before any hit window
            if (result == HitResult.None)
                return;

            // Hit before the Perfect window
            if (timeOffset < 0 && result is not HitResult.Perfect)
                return;

            ApplyResult(result);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);
            double time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    TouchBody.FadeOut();
                    this.FadeOut();
                    break;

                case ArmedState.Miss:
                    TouchBody.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                        .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                        .FadeOut(time_fade_miss);

                    this.Delay(time_fade_miss).FadeOut();
                    break;
            }
            Expire();
        }
    }
}
