using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
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

        // IsHovered is used
        public override bool HandlePositionalInput => true;

        // Similar to IsHovered for mouse, this tracks whether a pointer (touch or mouse) is interacting with this drawable
        // Interaction == (IsHovered && ActionPressed) || (OnTouch && TouchPointerInBounds)
        public bool[] PointInteractionState = new bool[11];
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

            Size = new Vector2(100);
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            AddRangeInternal(new Drawable[]
            {
                TouchBody = new TouchBody(),
            });

            trackedKeys.BindValueChanged(x =>
            {
                if (AllJudged)
                    return;

                UpdateResult(true);
            });
        }

        protected override void OnApply()
        {
            base.OnApply();
            Position = HitObject.Position;
        }

        protected override void OnFree()
        {
            base.OnFree();
            for (int i = 0; i < 11; ++i)
                PointInteractionState[i] = false;
        }

        private readonly BindableInt trackedKeys = new BindableInt();

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            double animTime = AnimationDuration.Value * 0.8;
            double fadeTime = AnimationDuration.Value * 0.2;

            TouchBody.FadeIn(fadeTime);

            using (BeginAbsoluteSequence(HitObject.StartTime - animTime))
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
                if (Auto && timeOffset > 0)
                    ApplyResult(HitResult.Perfect);
                else if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(Result.Judgement.MinResult);

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);

            // This is hit before any hit window
            if (result == HitResult.None)
                return;

            // Hit before the Perfect window
            if (timeOffset < 0 && result is not HitResult.Perfect)
                return;

            if (ExBindable.Value && result.IsHit())
                result = Result.Judgement.MaxResult;

            ApplyResult(result);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);
            double time_fade_miss = 400 * (DrawableSentakkiRuleset?.GameplaySpeed ?? 1);

            switch (state)
            {
                case ArmedState.Hit:
                    Expire();
                    break;

                case ArmedState.Miss:
                    this.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                        .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                        .FadeOut(time_fade_miss);
                    break;
            }
        }

        public bool OnNewPointInteraction() => UpdateResult(true);

        private struct TouchEasingFunction : IEasingFunction
        {
            public readonly double ApplyEasing(double t)
            {
                double result = 3.5 * Math.Pow(t, 4) - 3.75 * Math.Pow(t, 3) + 1.45 * Math.Pow(t, 2) - 0.05 * t + 0.005;

                return Math.Min(1, result);
            }
        }
    }
}
