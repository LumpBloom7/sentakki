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
                            NoteBody.Note.FadeColour(AccentColour.Value,50);
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
                        if(!AllJudged)
                        NoteBody.Note.FadeColour(Color4.Gray,100);
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

        protected override void UpdateInitialTransforms()
        {
            double animTime = AnimationDuration.Value / 2 * GameplaySpeed;
            double animStart = HitObject.StartTime - (animTime * 2);
            using (BeginAbsoluteSequence(animStart, true))
            {
                HitObjectLine.FadeInFromZero(animTime);
                NoteBody.FadeInFromZero(animTime).ScaleTo(1, animTime);

                using (BeginDelayedSequence(animTime, true))
                {
                    // This is the movable length (not including start position)
                    float totalMovableDistance = SentakkiPlayfield.INTERSECTDISTANCE - SentakkiPlayfield.NOTESTARTDISTANCE;
                    float originalStretchAmount = (float)(totalMovableDistance / animTime * (HitObject as IHasDuration).Duration);
                    float stretchAmount = Math.Clamp((float)(totalMovableDistance / animTime * (HitObject as IHasDuration).Duration), 0, totalMovableDistance);
                    float stretchTime = (float)(stretchAmount / totalMovableDistance * animTime);
                    float excessDistance = (float)((-SentakkiPlayfield.INTERSECTDISTANCE + SentakkiPlayfield.NOTESTARTDISTANCE) / animTime * 144);

                    NoteBody.ResizeHeightTo(80 + stretchAmount, stretchTime)
                            .Delay((HitObject as IHasDuration).Duration)
                            .MoveToY(-SentakkiPlayfield.INTERSECTDISTANCE + (Width / 2) + excessDistance, animTime + 144)
                            .Delay(animTime - stretchTime)
                            .ResizeHeightTo(80, stretchTime);

                    if (HoldStartTime == null)
                        NoteBody.Note.Delay(animTime).FadeColour(Color4.Gray, 100);

                    HitObjectLine.ScaleTo(1, animTime);
                }
            }
        }
        protected override void Update()
        {
            base.Update();
            if (Result.HasResult) return;

            // Let autoplay mimic players.
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

            switch (state)
            {
                case ArmedState.Hit:
                    using (BeginDelayedSequence((HitObject as IHasDuration).Duration + Tail.Result.TimeOffset, true))
                    {
                        this.ScaleTo(1f, time_fade_hit);
                        HitObjectLine.FadeOut();
                    }
                    break;

                case ArmedState.Miss:
                    using (BeginDelayedSequence((HitObject as IHasDuration).Duration + Tail.Result.TimeOffset, true))
                    {
                        NoteBody.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                            .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                            .MoveToOffset(new Vector2(0, -100), time_fade_hit, Easing.OutCubic)
                            .FadeOut(time_fade_miss);
                        HitObjectLine.FadeOut();

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
