using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;
using System;
using osu.Framework.Input.Bindings;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableHold : DrawableSentakkiLanedHitObject, IKeyBindingHandler<SentakkiAction>
    {
        private readonly Bindable<bool> isHitting = new Bindable<bool>();

        public DrawableHoldHead Head => headContainer.Child;

        private readonly Container<DrawableHoldHead> headContainer;

        public readonly HoldBody NoteBody;
        public readonly HitObjectLine HitObjectLine;

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        public double? HoldStartTime { get; private set; }

        public double TotalHoldTime = 0;

        public DrawableHold(Hold hitObject)
            : base(hitObject)
        {
            AccentColour.BindTo(HitObject.ColourBindable);
            Size = new Vector2(75);
            Position = Vector2.Zero;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AddRangeInternal(new Drawable[]{
                HitObjectLine = new HitObjectLine(),
                NoteBody = new HoldBody{
                    Duration = hitObject.Duration
                },
                headContainer = new Container<DrawableHoldHead> { RelativeSizeAxes = Axes.Both },
            });

            AccentColour.BindValueChanged(c => HitObjectLine.Colour = c.NewValue);
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
            headContainer.Clear();
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
                        AccentColour = { BindTarget = AccentColour },
                        AutoBindable = { BindTarget = AutoBindable }
                    };
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            double animTime = AdjustedAnimationDuration / 2;
            HitObjectLine.FadeInFromZero(animTime);
            NoteBody.FadeInFromZero(animTime).ScaleTo(1, animTime);

            using (BeginDelayedSequence(animTime, true))
            {
                // This is the movable length (not including start position)
                float totalMovableDistance = SentakkiPlayfield.INTERSECTDISTANCE - SentakkiPlayfield.NOTESTARTDISTANCE;
                float originalStretchAmount = (float)(totalMovableDistance / animTime * (HitObject as IHasDuration).Duration);
                float stretchAmount = Math.Clamp((float)(totalMovableDistance / animTime * (HitObject as IHasDuration).Duration), 0, totalMovableDistance);
                float stretchTime = (float)(stretchAmount / totalMovableDistance * animTime);
                float excessDistance = (float)((-SentakkiPlayfield.INTERSECTDISTANCE + SentakkiPlayfield.NOTESTARTDISTANCE) / animTime);

                NoteBody.ResizeHeightTo(75 + stretchAmount, stretchTime)
                        .Delay((HitObject as IHasDuration).Duration)
                        .MoveToY(-SentakkiPlayfield.INTERSECTDISTANCE + (Width / 2), animTime)
                        .Delay(animTime - stretchTime)
                        .ResizeHeightTo(75, stretchTime);

                if (HoldStartTime == null && !Auto)
                    NoteBody.Note.Delay(animTime).FadeColour(Color4.Gray, 100);

                HitObjectLine.ScaleTo(1, animTime);
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (!userTriggered)
            {
                if (Time.Current > HitObject.GetEndTime())
                {
                    endHold();
                    double totalHoldRatio = TotalHoldTime / ((IHasDuration)HitObject).Duration;
                    HitResult result;

                    if (totalHoldRatio >= .75)
                        result = HitResult.Great;
                    else if (totalHoldRatio >= .5)
                        result = HitResult.Good;
                    else if (totalHoldRatio >= .25)
                        result = HitResult.Meh;
                    else
                        result = HitResult.Miss;

                    // Hold is over, but head windows are still active.
                    // Only happens on super short holds
                    // Force a miss on the head in this case
                    if (!headContainer.First().Result.HasResult)
                        headContainer.First().MissForcefully();

                    if (Auto) result = HitResult.Great;
                    ApplyResult(r => r.Type = result);
                }
            }
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);
            const double time_fade_hit = 400, time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    using (BeginDelayedSequence((HitObject as IHasDuration).Duration, true))
                    {
                        HitObjectLine.FadeOut();
                        using (BeginDelayedSequence(time_fade_miss, true))
                        {
                            this.FadeOut();
                            Expire();
                        }
                    }
                    break;

                case ArmedState.Miss:
                    using (BeginDelayedSequence((HitObject as IHasDuration).Duration, true))
                    {
                        NoteBody.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                            .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                            .MoveToOffset(new Vector2(0, -100), time_fade_hit, Easing.OutCubic)
                            .FadeOut(time_fade_miss);
                        HitObjectLine.FadeOut();

                        using (BeginDelayedSequence(time_fade_miss, true))
                        {
                            this.FadeOut();
                            Expire();
                        }
                    }
                    break;
            }
        }

        private bool beginHoldAt(double timeOffset)
        {
            if (timeOffset < -Head.HitObject.HitWindows.WindowFor(HitResult.Miss))
                return false;

            HoldStartTime = Math.Max(Time.Current, HitObject.StartTime);
            isHitting.Value = true;
            return true;
        }

        private void endHold()
        {
            if (HoldStartTime.HasValue)
                TotalHoldTime += Math.Max(Time.Current - HoldStartTime.Value, 0);

            HoldStartTime = null;
            isHitting.Value = false;
        }

        public virtual bool OnPressed(SentakkiAction action)
        {
            if (AllJudged)
                return false;

            if (action != SentakkiAction.Key1 + ((SentakkiLanedHitObject)HitObject).Lane)
                return false;

            if (beginHoldAt(Time.Current - Head.HitObject.StartTime))
            {
                Head.UpdateResult();
                NoteBody.Note.FadeColour(AccentColour.Value, 50);
            }

            return true;
        }

        public void OnReleased(SentakkiAction action)
        {
            if (AllJudged) return;
            if (HoldStartTime is null) return;

            if (action != SentakkiAction.Key1 + ((SentakkiLanedHitObject)HitObject).Lane)
                return;

            endHold();

            if (!AllJudged)
                NoteBody.Note.FadeColour(Color4.Gray, 100);
        }
    }
}
