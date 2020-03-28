// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets.Maimai.Configuration;
using osu.Game.Rulesets.Maimai.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Maimai.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;
using System;
using System.Linq;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables
{
    public class DrawableHold : DrawableMaimaiHitObject
    {
        private readonly HitReceptor HitArea;
        private readonly HoldBody note;
        public readonly CircularContainer HitObjectLine;
        protected override double InitialLifetimeOffset => 3500;

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
                HitArea = new HitReceptor
                {
                    Position = new Vector2(0,-MaimaiPlayfield.IntersectDistance),
                }
            });
        }

        Bindable<double> AnimationDuration = new Bindable<double>(1000);

        [BackgroundDependencyLoader(true)]
        private void load(MaimaiRulesetConfigManager settings)
        {
            settings?.BindWith(MaimaiRulesetSettings.AnimationDuration, AnimationDuration);
            HitObjectLine.Child.Colour = HitObject.NoteColor;
        }

        double fadeIn = 500, moveTo, idle;
        protected override void UpdateInitialTransforms()
        {
            AnimationDuration.TriggerChange();
            fadeIn = 500;
            moveTo = AnimationDuration.Value;
            idle = 3500 - fadeIn - moveTo;

            float originalHeight = 40;
            float length = Convert.ToSingle((MaimaiPlayfield.IntersectDistance - 66) / AnimationDuration.Value * ((HitObject as IHasEndTime).Duration));
            double extendTime = (length / (MaimaiPlayfield.IntersectDistance - 66)) * AnimationDuration.Value;

            var seq = note.Delay(idle)
                    .FadeInFromZero(500)
                    .ScaleTo(1f, fadeIn)
                    .Then();

                if (length >= (MaimaiPlayfield.IntersectDistance - 66))
                    seq.ResizeHeightTo(MaimaiPlayfield.IntersectDistance - 66 + 80, moveTo)
                    .Delay((HitObject as IHasEndTime).Duration)
                    .ResizeHeightTo(80, moveTo)
                    .MoveToY(-(MaimaiPlayfield.IntersectDistance - 40), moveTo);
                else
                    seq.ResizeHeightTo(length + 80, extendTime)
                    .Then()
                    .MoveToY(-(MaimaiPlayfield.IntersectDistance - length - 80 + 40), AnimationDuration.Value - extendTime)
                    .Then()
                    .ResizeHeightTo(80, extendTime)
                    .MoveToY(-(MaimaiPlayfield.IntersectDistance - 40), extendTime);


            HitObjectLine.Delay(idle).FadeTo(.75f, fadeIn).Then().ResizeTo(600, moveTo);

            if (isHidden)
                this.Delay(idle + fadeIn).FadeOut(moveTo / 2);
        }

        private double potential = 0;
        private double held = 0;
        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current < HitObject.StartTime) return;

            if (userTriggered || Time.Current < (HitObject as IHasEndTime).EndTime)
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
                else if (Time.Current >= (HitObject as IHasEndTime).EndTime)
                    r.Type = HitResult.Miss;
            });
        }

        public bool Auto = false;
        protected override void Update()
        {
            if (Time.Current >= HitObject.StartTime && Time.Current <= (HitObject as IHasEndTime).EndTime)
            {
                potential++;
                if (HitArea.IsDown() || Auto)
                {
                    held++;
                    this.FadeTo(isHidden ? .2f : 1f, 100);
                    //explode.FadeTo(1f, 100);
                }
                else
                {
                    this.FadeTo(isHidden ? 0f : .5f, 200);
                    //explode.FadeTo(0f, 200);
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

        public class HitReceptor : CircularContainer
        {
            // IsHovered is used
            public override bool HandlePositionalInput => true;

            private MaimaiInputManager maimaiActionInputManager;
            internal MaimaiInputManager MaimaiActionInputManager => maimaiActionInputManager ??= GetContainingInputManager() as MaimaiInputManager;

            public bool IsDown() => IsHovered && (MaimaiActionInputManager?.PressedActions.Any(x => x == MaimaiAction.Button1 || x == MaimaiAction.Button2) ?? false);

            public MaimaiAction? HitAction;
            public HitReceptor()
            {
                RelativeSizeAxes = Axes.None;
                Size = new Vector2(350f);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }
        }
    }
}
