// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
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
using System.Collections.Generic;
using System.Linq;

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
        public readonly CircularContainer HitObjectLine;
        protected override double InitialLifetimeOffset => 3500;

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
            AddRangeInternal(new Drawable[]{
                HitObjectLine = new HitObjectLine(),
                note = new HoldBody{
                    Duration = hitObject.Duration
                },
                headContainer = new Container<DrawableHoldHead> { RelativeSizeAxes = Axes.Both },
                tailContainer = new Container<DrawableHoldTail> { RelativeSizeAxes = Axes.Both },
                HitArea = new HitReceptor
                {
                    Position = new Vector2(0,-SentakkiPlayfield.IntersectDistance),
                    Hit = () => {
                        if (AllJudged)
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
                    }
                }
            });
        }

        protected override void Update()
        {
            if (Time.Current >= HitObject.StartTime)
            {
                if (isHitting.Value)
                {
                    note.glow.FadeIn(50);
                    this.FadeTo(IsHidden ? .2f : 1f, 50);
                }
                else
                {
                    note.glow.FadeOut(100);
                    this.FadeTo(IsHidden ? 0f : .5f, 200);
                }
            }
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
            HitObjectLine.Child.Colour = HitObject.NoteColor;
        }

        private double fadeIn = 500, moveTo, idle;

        protected override void UpdateInitialTransforms()
        {
            animationDuration.TriggerChange();
            fadeIn = 500;
            moveTo = animationDuration.Value;
            idle = 3500 - fadeIn - moveTo;

            float length = Convert.ToSingle((SentakkiPlayfield.IntersectDistance - 66) / animationDuration.Value * ((HitObject as IHasEndTime).Duration));
            double extendTime = (length / (SentakkiPlayfield.IntersectDistance - 66)) * animationDuration.Value;

            var seq = note.Delay(idle)
                    .FadeInFromZero(500)
                    .ScaleTo(1f, fadeIn)
                    .Then();

            if (length >= (SentakkiPlayfield.IntersectDistance - 66))
                seq.ResizeHeightTo(SentakkiPlayfield.IntersectDistance - 66 + 80, moveTo)
                .Delay((HitObject as IHasEndTime).Duration)
                .ResizeHeightTo(80, moveTo)
                .MoveToY(-(SentakkiPlayfield.IntersectDistance - 40), moveTo);
            else
                seq.ResizeHeightTo(length + 80, extendTime)
                .Then()
                .MoveToY(-(SentakkiPlayfield.IntersectDistance - length - 80 + 40), animationDuration.Value - extendTime)
                .Then()
                .ResizeHeightTo(80, extendTime)
                .MoveToY(-(SentakkiPlayfield.IntersectDistance - 40), extendTime);

            HitObjectLine.Delay(idle).FadeTo(.75f, fadeIn).Then().ResizeTo(600, moveTo);

            if (IsHidden)
                this.Delay(idle + fadeIn).FadeOut(moveTo / 2);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Tail.AllJudged)
                ApplyResult(r => r.Type = HitResult.Perfect);
        }

        public bool Auto = false;

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);
            const double time_fade_hit = 400, time_fade_miss = 400;
            switch (state)
            {
                case ArmedState.Hit:
                    HitObjectLine.FadeOut();
                    using (BeginDelayedSequence((HitObject as IHasEndTime).Duration, true))
                    {
                        this.ScaleTo(1f, time_fade_hit);
                    }
                    break;

                case ArmedState.Miss:
                    var c = HitObject.Angle + 90;
                    var d = c * (float)(Math.PI / 180);

                    using (BeginDelayedSequence((HitObject as IHasEndTime).Duration, true))
                    {
                        note.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                            .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                            .MoveToOffset(new Vector2(0, -100), time_fade_hit, Easing.OutCubic)
                            .FadeOut(time_fade_miss);

                        using (BeginDelayedSequence(time_fade_miss, true))
                        {
                            this.Expire();
                        }
                    }
                    HitObjectLine.FadeOut();
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

        public class HitReceptor : CircularContainer, IKeyBindingHandler<SentakkiAction>
        {
            // IsHovered is used
            public override bool HandlePositionalInput => true;

            private SentakkiInputManager sentakkiActionInputManager;
            internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

            private List<SentakkiAction> actions = new List<SentakkiAction>();
            public Func<bool> Hit;
            public Action Release;

            public HitReceptor()
            {
                RelativeSizeAxes = Axes.None;
                Size = new Vector2(350);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            public bool OnPressed(SentakkiAction action)
            {
                switch (action)
                {
                    case SentakkiAction.Button1:
                    case SentakkiAction.Button2:
                        if (IsHovered && (Hit?.Invoke() ?? false))
                        {
                            actions.Add(action);
                            return true;
                        }
                        break;
                }

                return false;
            }

            public void OnReleased(SentakkiAction action)
            {
                switch (action)
                {
                    case SentakkiAction.Button1:
                    case SentakkiAction.Button2:
                        if (actions.Contains(action))
                            actions.Remove(action);
                        if (!actions.Any())
                            Release?.Invoke();
                        break;
                }
            }
        }
    }
}
