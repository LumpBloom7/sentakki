using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableHold : DrawableSentakkiLanedHitObject, IKeyBindingHandler<SentakkiAction>
    {
        public new Hold HitObject => (Hold)base.HitObject;
        public DrawableHoldHead Head => headContainer.Child;

        private Container<DrawableHoldHead> headContainer = null!;

        public HoldBody NoteBody = null!;

        public override double LifetimeStart
        {
            get => base.LifetimeStart;
            set
            {
                base.LifetimeStart = value;
                NoteBody.LifetimeStart = value;
            }
        }

        public override double LifetimeEnd
        {
            get => base.LifetimeEnd;
            set
            {
                base.LifetimeEnd = value;
                NoteBody.LifetimeEnd = value;
            }
        }

        public DrawableHold()
            : this(null)
        {
        }

        public DrawableHold(Hold? hitObject = null)
            : base(hitObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AddRangeInternal(
            [
                NoteBody = new HoldBody(),
                headContainer = new Container<DrawableHoldHead> { RelativeSizeAxes = Axes.Both },
            ]);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            AccentColour.BindValueChanged(c => flashingColour = AccentColour.Value.LightenHSL(0.4f), true);
        }

        private Color4 flashingColour = Color4.White;

        protected override void OnApply()
        {
            base.OnApply();
            timeNotHeld = 0;
            isHolding = false;
        }

        private double timeNotHeld = 0;

        private bool isHolding;

        protected override void Update()
        {
            base.Update();

            if (AllJudged)
            {
                // Remove alterations to NoteBody colour
                NoteBody.Colour = AccentColour.Value;
                return;
            }

            // Ensure that the note colour is correct prior to the start time
            if (Time.Current < HitObject.StartTime)
            {
                Colour = Color4.White;
                NoteBody.Colour = AccentColour.Value;
                return;
            }

            if (!isHolding && !Auto)
            {
                // Remove alterations to NoteBody colour
                NoteBody.Colour = AccentColour.Value;

                // Grey the note to indicate that it isn't being held
                Colour = Interpolation.ValueAt(
                    Math.Clamp(Time.Current, HitObject.StartTime, HitObject.StartTime + 100),
                    Color4.White, Color4.SlateGray,
                    HitObject.StartTime, HitObject.StartTime + 100, Easing.OutSine);

                if (Head.AllJudged)
                    timeNotHeld = Math.Clamp(timeNotHeld + Time.Elapsed, 0, HitObject.Duration);
                return;
            }

            // Restore colour if it is being held
            Colour = Color4.White;

            const double flashing_time = 80;

            double flashProg = (Time.Current % (flashing_time * 2)) / (flashing_time * 2);

            if (flashProg <= 0.5)
                NoteBody.Colour = Interpolation.ValueAt(flashProg, AccentColour.Value, flashingColour, 0, 0.5, Easing.OutSine);
            else
                NoteBody.Colour = Interpolation.ValueAt(flashProg, flashingColour, AccentColour.Value, 0.5, 0, Easing.InSine);
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            double animTime = AnimationDuration.Value / 2;
            NoteBody.FadeInFromZero(animTime).ScaleTo(1, animTime);

            using (BeginDelayedSequence(animTime))
            {
                // This is the movable length (not including start position)
                const float total_movable_distance = SentakkiPlayfield.INTERSECTDISTANCE - SentakkiPlayfield.NOTESTARTDISTANCE;

                // This is the amount of stretch needed. Capped to the max stretch amount.
                float stretchAmount = Math.Clamp((float)(total_movable_distance / animTime * (HitObject as IHasDuration).Duration), 0, total_movable_distance);

                // This is the amount of time that the note spends stretching or unstretching
                float stretchTime = (float)(stretchAmount / total_movable_distance * animTime);

                NoteBody.MoveToY(-SentakkiPlayfield.INTERSECTDISTANCE, animTime) // Move the head towards the ring
                        .ResizeHeightTo(stretchAmount, stretchTime) // While we are moving, we stretch the hold note to match desired length
                        .Then().Delay(HitObject.Duration - stretchTime) // Wait until the end of the hold note, while considering how much time we need for shrinking
                        .ResizeHeightTo(0, stretchTime); // We shrink the hold note as it exits
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (!userTriggered)
            {
                if (timeOffset >= 0 && Auto)
                    ApplyResult(HitResult.Perfect);
                else if (timeOffset > HitObject.HitWindows.WindowFor(HitResult.Perfect) && isHolding)
                    ApplyResult(applyDeductionTo(HitResult.Great));
                else if (Head.AllJudged && timeOffset >= 0 && !isHolding)
                    ApplyResult(Result.Judgement.MinResult);

                return;
            }
            var result = HitObject.HitWindows.ResultFor(timeOffset);

            if (result == HitResult.None)
                return;

            ApplyResult(applyDeductionTo(result));

            HitResult applyDeductionTo(HitResult originalResult)
            {
                int deduction = (int)Math.Clamp(Math.Floor(timeNotHeld / 300), 0, 3);

                var newResult = originalResult - deduction;

                if (newResult <= HitResult.Ok)
                    return HitResult.Ok;

                return newResult;
            }
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);
            double time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    NoteBody.FadeOut();
                    break;
                case ArmedState.Miss:
                    NoteBody.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                            .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                            .MoveToOffset(new Vector2(0, -100), time_fade_miss, Easing.OutCubic)
                            .FadeOut(time_fade_miss);

                    this.Delay(time_fade_miss).FadeOut();
                    break;
            }
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Hold.HoldHead head:
                    return new DrawableHoldHead(head)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AutoBindable = { BindTarget = AutoBindable }
                    };
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);

            switch (hitObject)
            {
                case DrawableHoldHead head:
                    headContainer.Child = head;
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            headContainer.Clear(false);
        }

        private SentakkiInputManager sentakkiActionInputManager = null!;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= (SentakkiInputManager)GetContainingInputManager();

        private int pressedCount
        {
            get
            {
                int count = 0;
                foreach (var pressedAction in SentakkiActionInputManager.PressedActions)
                {
                    if (pressedAction == SentakkiAction.Key1 + HitObject.Lane)
                        ++count;
                }

                return count;
            }
        }


        public bool OnPressed(KeyBindingPressEvent<SentakkiAction> e)
        {
            if (AllJudged)
                return false;

            if (e.Action != SentakkiAction.Key1 + HitObject.Lane)
                return false;

            // Passthrough excess inputs to later hitobjects in the same lane
            if (isHolding)
                return false;

            double timeOffset = Time.Current - HitObject.StartTime;

            if (timeOffset < -Head.HitObject.HitWindows.WindowFor(HitResult.Miss))
                return false;

            Head.UpdateResult();
            isHolding = true;
            NoteBody.FadeColour(AccentColour.Value, 50);
            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<SentakkiAction> e)
        {
            if (AllJudged) return;
            if (!isHolding) return;

            if (e.Action != SentakkiAction.Key1 + HitObject.Lane)
                return;

            // We only release the hold once ALL inputs are released
            // We check for 1 here as drawables receive the event before the counter decrements
            if (pressedCount > 1)
                return;

            UpdateResult(true);
            isHolding = false;

            if (!AllJudged)
                NoteBody.FadeColour(Color4.Gray, 100);
        }
    }
}
