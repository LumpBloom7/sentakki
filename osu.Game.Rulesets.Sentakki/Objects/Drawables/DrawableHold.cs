using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
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
            AddRangeInternal(new Drawable[]
            {
                NoteBody = new HoldBody(),
                headContainer = new Container<DrawableHoldHead> { RelativeSizeAxes = Axes.Both },
            });
        }

        protected override void OnFree()
        {
            base.OnFree();
            HoldStartTime = null;
            TotalHoldTime = 0;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            double animTime = AnimationDuration.Value / 2;
            NoteBody.FadeInFromZero(animTime).ScaleTo(1, animTime);

            NoteBody.FadeColour(AccentColour.Value);

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

                if (HoldStartTime == null && !Auto)
                    NoteBody.Delay(animTime).FadeColour(Color4.Gray, 100);
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current > HitObject.GetEndTime())
            {
                endHold();
                double totalHoldRatio = TotalHoldTime / ((IHasDuration)HitObject).Duration;
                HitResult result;

                if (totalHoldRatio >= .75 || Auto)
                    result = HitResult.Great;
                else if (totalHoldRatio >= .5)
                    result = HitResult.Good;
                else if (totalHoldRatio >= .25)
                    result = HitResult.Ok;
                else
                    result = HitResult.Miss;

                // This is specifically to accommodate the threshold setting in HR
                if (!HitObject.HitWindows.IsHitResultAllowed(result))
                    result = HitResult.Miss;

                // Hold is over, but head windows are still active.
                // Only happens on super short holds
                // Force a miss on the head in this case
                if (!headContainer[0].Result.HasResult)
                    headContainer[0].MissForcefully();

                ApplyResult(result);
            }
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
                    NoteBody.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                            .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                            .MoveToOffset(new Vector2(0, -100), time_fade_miss, Easing.OutCubic)
                            .FadeOut(time_fade_miss);

                    using (BeginDelayedSequence(time_fade_miss))
                        this.FadeOut();
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

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        public double? HoldStartTime { get; private set; }

        public double TotalHoldTime;

        private bool beginHoldAt(double timeOffset)
        {
            if (HoldStartTime is not null)
                return false;

            if (timeOffset < -Head.HitObject.HitWindows.WindowFor(HitResult.Miss))
                return false;

            HoldStartTime = Math.Max(Time.Current, HitObject.StartTime);
            return true;
        }

        private void endHold()
        {
            if (HoldStartTime.HasValue)
                TotalHoldTime += Math.Max(Time.Current - HoldStartTime.Value, 0);

            HoldStartTime = null;
        }

        private SentakkiInputManager sentakkiActionInputManager = null!;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= (SentakkiInputManager)GetContainingInputManager();

        private int pressedCount
        {
            get
            {
                int count = 0;
                foreach (var pressedAction in sentakkiActionInputManager.PressedActions)
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

            if (beginHoldAt(Time.Current - Head.HitObject.StartTime))
            {
                Head.UpdateResult();
                NoteBody.FadeColour(AccentColour.Value, 50);
                return true;
            }

            // Passthrough excess inputs to later hitobjects in the same lane
            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<SentakkiAction> e)
        {
            if (AllJudged) return;
            if (HoldStartTime is null) return;

            if (e.Action != SentakkiAction.Key1 + HitObject.Lane)
                return;

            // We only release the hold once ALL inputs are released
            // We check for 1 here as drawables receive the event before the counter decrements
            if (pressedCount > 1)
                return;

            endHold();

            if (!AllJudged)
                NoteBody.FadeColour(Color4.Gray, 100);
        }
    }
}
