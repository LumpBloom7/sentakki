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
        public readonly HoldBody NoteBody;
        public readonly HitObjectLine HitObjectLine;
        protected override double InitialLifetimeOffset => 8000;

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        public double? HoldStartTime { get; private set; }

        protected override bool PlayBreakSample => false;

        public DrawableHold(Hold hitObject)
            : base(hitObject)
        {
            AccentColour.Value = hitObject.NoteColor;
            Size = new Vector2(80);
            Position = Vector2.Zero;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AlwaysPresent = true;
            AddRangeInternal(new Drawable[]{
                HitObjectLine = new HitObjectLine(),
                NoteBody = new HoldBody{
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

                        if(beginHoldAt(Time.Current - Head.HitObject.StartTime))
                        {
                            Head.UpdateResult();
                            NoteBody.Glow.FadeIn(50);
                        }

                        return true;
                    },
                    Release = () =>
                    {
                        if(AllJudged) return;
                        if(HoldStartTime is null) return;

                        Tail.UpdateResult();
                        HoldStartTime = null;
                        isHitting.Value = false;
                        NoteBody.Glow.FadeOut(100);
                    },
                }
            });

            hitObject.LaneBindable.BindValueChanged(r =>
            {
                Rotation = r.NewValue.GetRotationForLane();
                HitArea.NotePath = r.NewValue;
            }, true);
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
                case Hold.HoldTail _:
                    return new DrawableHoldTail(this)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AccentColour = { BindTarget = AccentColour }
                    };

                case Hold.HoldHead _:
                    return new DrawableHoldHead(this)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AccentColour = { BindTarget = AccentColour }
                    };
            }

            return base.CreateNestedHitObject(hitObject);
        }

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager settings)
        {
            settings?.BindWith(SentakkiRulesetSettings.AnimationDuration, AnimationDuration);
            HitObjectLine.Colour = HitObject.NoteColor;
        }

        protected override void Update()
        {
            base.Update();
            if (Result.HasResult) return;

            double animTime = AnimationDuration.Value / 2 * GameplaySpeed;
            double animStart = HitObject.StartTime - (animTime * 2);
            double currentProg = Clock.CurrentTime - animStart;

            // Calculate initial entry animation
            float fadeAmount = Math.Clamp((float)(currentProg / animTime), 0, 1);
            HitObjectLine.Alpha = fadeAmount;
            NoteBody.Alpha = fadeAmount;
            NoteBody.Scale = new Vector2(fadeAmount);

            // This is the movable length (not including start position)
            float totalMovableDistance = SentakkiPlayfield.INTERSECTDISTANCE - SentakkiPlayfield.NOTESTARTDISTANCE;
            float adjustedStartPoint = SentakkiPlayfield.NOTESTARTDISTANCE - (Width / 2);

            // Calculate total length of hold note
            double length = (float)(totalMovableDistance / animTime * (HitObject as IHasDuration).Duration);
            if (length > totalMovableDistance) // Clip max length
                length = totalMovableDistance;

            // Calculate time taken to extend to desired length
            double extendTime = length / totalMovableDistance * animTime;

            // Start strecthing
            float extendAmount = Math.Clamp((float)((currentProg - animTime) / extendTime), 0, 1);
            NoteBody.Height = (float)(80 + (length * extendAmount));

            // Move the note once idle time is over
            float moveAmount = Math.Clamp((float)((currentProg - animTime - (HitObject as IHasDuration).Duration) / animTime), 0, 1);
            NoteBody.Y = -adjustedStartPoint - (totalMovableDistance * moveAmount);

            // Start shrinking when the time comes
            float shrinkAmount = Math.Abs(NoteBody.Y) + NoteBody.Height - SentakkiPlayfield.INTERSECTDISTANCE - 40;
            if (shrinkAmount > 0)
                NoteBody.Height -= shrinkAmount;

            // Handle hidden and fadeIn modifications
            if (IsHidden)
            {
                float hideAmount = Math.Clamp((float)((currentProg - animTime) / (animTime / 2)), 0, 1);
                // Provide feedback, and allow users to see the length of the note
                if (Time.Current >= HitObject.StartTime && (isHitting.Value || Auto))
                    hideAmount /= 2;

                Alpha = 1 - hideAmount;
            }
            else if (IsFadeIn)
            {
                float fadeInAmount = Math.Clamp((float)((currentProg - animTime) / animTime), 0, 1);
                Alpha = 1 * fadeInAmount;
            }

            // Make sure HitObjectLine is adjusted with the moving note
            float totalMove = Math.Clamp((float)((currentProg - animTime) / animTime), 0, 1);

            HitObjectLine.UpdateVisual(totalMove);

            // Auto should pretend to trigger a hit, just so it visually looks the same even if the note is guaranteed to give a perfect judgement
            if (Auto)
                if (Time.Current >= HitObject.StartTime)
                    HitArea.Hit.Invoke();
                else
                    HitArea.Release.Invoke(); //Make sure note is released...To avoid autoplay rewind visual issue.
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
                    using (BeginAbsoluteSequence(Time.Current, true))
                    {
                        this.ScaleTo(1f, time_fade_hit);
                    }
                    break;

                case ArmedState.Miss:
                    using (BeginAbsoluteSequence(Time.Current, true))
                    {
                        NoteBody.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
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

        private bool beginHoldAt(double timeOffset)
        {
            if (timeOffset < -Head.HitObject.HitWindows.WindowFor(HitResult.Miss))
                return false;

            HoldStartTime = Time.Current;
            isHitting.Value = true;
            return true;
        }
    }
}
