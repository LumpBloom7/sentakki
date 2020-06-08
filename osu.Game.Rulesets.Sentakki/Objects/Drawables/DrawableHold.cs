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
namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableHold : DrawableSentakkiHitObject
    {
        public override bool DisplayResult => false;

        public IBindable<bool> IsHitting => isHitting;

        private readonly Bindable<bool> isHitting = new Bindable<bool>();

        public DrawableHoldHead Head => headContainer.Child;
        public DrawableHoldTail Tail => tailContainer.Child;

        private readonly Container<DrawableHoldHead> headContainer;
        private readonly Container<DrawableHoldTail> tailContainer;

        public readonly HitReceptor HitArea;
        private readonly HoldBody note;
        public readonly HitObjectLine HitObjectLine;
        protected override double InitialLifetimeOffset => 8000;

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        public double? HoldStartTime { get; private set; }

        public DrawableHold(Hold hitObject)
            : base(hitObject)
        {
            AccentColour.Value = hitObject.NoteColor;
            Size = new Vector2(80);
            Position = Vector2.Zero;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Rotation = HitObject.Angle;
            AlwaysPresent = true;
            AddRangeInternal(new Drawable[]{
                HitObjectLine = new HitObjectLine(),
                note = new HoldBody{
                    Duration = hitObject.Duration
                },
                headContainer = new Container<DrawableHoldHead> { RelativeSizeAxes = Axes.Both },
                tailContainer = new Container<DrawableHoldTail> { RelativeSizeAxes = Axes.Both },
                HitArea = new HitReceptor
                {
                    Position = new Vector2(0,-SentakkiPlayfield.INTERSECTDISTANCE),
                    Hit = () => {
                        if (AllJudged || HoldStartTime != null)
                            return false;
                        beginHoldAt(Time.Current - Head.HitObject.StartTime);
                        Head.UpdateResult();

                        return true;
                    },
                    Release = () =>
                    {
                        if(AllJudged) return;
                        if(HoldStartTime is null) return;

                        Tail.UpdateResult();
                        HoldStartTime = null;
                        isHitting.Value = false;
                    },
                    NoteAngle = HitObject.Angle
                }
            });
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);

            switch (hitObject)
            {
                case DrawableHoldHead head:
                    headContainer.Child = head;
                    break;

                case DrawableHoldTail tail:
                    tailContainer.Child = tail;
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            headContainer.Clear();
            tailContainer.Clear();
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case HoldTail _:
                    return new DrawableHoldTail(this)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AccentColour = { BindTarget = AccentColour }
                    };

                case Tap _:
                    return new DrawableHoldHead(this)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AccentColour = { BindTarget = AccentColour }
                    };
            }

            return base.CreateNestedHitObject(hitObject);
        }

        private Bindable<double> animationDuration = new Bindable<double>(1000);

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager settings)
        {
            settings?.BindWith(SentakkiRulesetSettings.AnimationDuration, animationDuration);
            HitObjectLine.Colour = HitObject.NoteColor;
        }

        protected override void Update()
        {
            base.Update();
            if (Result.HasResult) return;

            double animSpeed = animationDuration.Value / 2;
            double fadeIn = animSpeed * GameplaySpeed;
            double moveTo = animSpeed * GameplaySpeed;
            double animStart = HitObject.StartTime - moveTo - fadeIn;
            double currentProg = Clock.CurrentTime - animStart;

            // Calculate initial entry animation
            float fadeAmount = (float)(currentProg / fadeIn);
            if (fadeAmount < 0) fadeAmount = 0;
            else if (fadeAmount > 1) fadeAmount = 1;

            HitObjectLine.Alpha = fadeAmount;
            note.Alpha = (float)(1 * fadeAmount);
            note.Scale = new Vector2((float)(1 * fadeAmount));

            // Calculate total length of hold note
            double length = Convert.ToSingle((SentakkiPlayfield.INTERSECTDISTANCE - 66) / moveTo * ((HitObject as IHasDuration).Duration));
            if (length > SentakkiPlayfield.INTERSECTDISTANCE - 66) // Clip max length
                length = SentakkiPlayfield.INTERSECTDISTANCE - 66;

            // Calculate time taken to extend to desired length
            double extendTime = length / (SentakkiPlayfield.INTERSECTDISTANCE - 66) * moveTo;

            // Start strecthing
            float extendAmount = (float)((currentProg - fadeIn) / extendTime);
            if (extendAmount < 0) extendAmount = 0;
            else if (extendAmount > 1) extendAmount = 1;

            note.Height = (float)(80 + (length * extendAmount));

            // Calculate duration where no movement is happening (when notes are very long)
            float idleTime = (float)((HitObject as IHasDuration).Duration - extendTime);

            // Move the note once idle time is over
            float moveAmount = (float)((currentProg - fadeIn - extendTime - idleTime) / moveTo);
            if (moveAmount < 0) moveAmount = 0;
            else if (moveAmount > 1) moveAmount = 1;

            float yDiff = SentakkiPlayfield.INTERSECTDISTANCE - 66;
            note.Y = -26 - (yDiff * moveAmount);

            // Start shrinking when the time comes
            float shrinkAmount = Math.Abs(note.Y) + note.Height - SentakkiPlayfield.INTERSECTDISTANCE - 40;
            if (shrinkAmount > 0)
                note.Height -= shrinkAmount;

            // Handle hidden and fadeIn modifications
            if (IsHidden)
            {
                float hideAmount = (float)((currentProg - fadeIn) / (moveTo / 2));
                if (hideAmount < 0) hideAmount = 0;
                else if (hideAmount > 1) hideAmount = 1;

                Alpha = 1 - (1 * hideAmount / ((Time.Current >= HitObject.StartTime && (isHitting.Value || Auto)) ? 2 : 1));
            }
            else if (IsFadeIn)
            {
                float fadeInAmount = (float)((currentProg - fadeIn) / moveTo);
                if (fadeInAmount < 0) fadeInAmount = 0;
                else if (fadeInAmount > 1) fadeInAmount = 1;

                Alpha = 1 * fadeInAmount;
            }

            // Make sure HitObjectLine is adjusted with the moving note
            float totalMove = (float)((currentProg - fadeIn) / moveTo);
            if (totalMove < 0) totalMove = 0;
            else if (totalMove > 1) totalMove = 1;

            HitObjectLine.UpdateVisual(totalMove);

            // Hit feedback glow
            if (Time.Current >= HitObject.StartTime)
            {
                if (isHitting.Value || Auto)
                    note.Glow.FadeIn(50);
                else
                    note.Glow.FadeOut(100);
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Tail.AllJudged)
                ApplyResult(r => r.Type = (Head.IsHit || Tail.IsHit) ? HitResult.Perfect : HitResult.Miss);
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);
            const double time_fade_hit = 400, time_fade_miss = 400;
            HitObjectLine.FadeOut();

            switch (state)
            {
                case ArmedState.Hit:
                    using (BeginDelayedSequence((HitObject as IHasDuration).Duration, true))
                    {
                        this.ScaleTo(1f, time_fade_hit);
                    }
                    break;

                case ArmedState.Miss:
                    double longestSurvivalTime = Tail.HitObject.HitWindows.WindowFor(HitResult.Miss);
                    using (BeginDelayedSequence((HitObject as IHasDuration).Duration + longestSurvivalTime, true))
                    {
                        note.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                            .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                            .MoveToOffset(new Vector2(0, -100), time_fade_hit, Easing.OutCubic)
                            .FadeOut(time_fade_miss);

                        using (BeginDelayedSequence(time_fade_miss, true))
                        {
                            Expire();
                        }
                    }
                    break;
            }
        }

        private void beginHoldAt(double timeOffset)
        {
            if (timeOffset < -Head.HitObject.HitWindows.WindowFor(HitResult.Miss))
                return;

            HoldStartTime = Time.Current;
            isHitting.Value = true;
        }
    }
}
