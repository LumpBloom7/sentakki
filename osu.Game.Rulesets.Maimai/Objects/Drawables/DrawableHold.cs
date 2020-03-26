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
        //private readonly FlashPiece flash;
        private readonly ExplodePiece explode;
        private readonly HitReceptor HitArea;
        private readonly Container note;
        public readonly CircularProgress HitObjectLine;
        private readonly FlashPiece flash;
        protected override double InitialLifetimeOffset => 3500;

        public DrawableHold(Hold hitObject)
            : base(hitObject)
        {
            Size = new Vector2(80);
            Position = Vector2.Zero;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Rotation = HitObject.Angle;
            AddRangeInternal(new Drawable[]{
                HitObjectLine = new CircularProgress
                {
                    Size = new Vector2(MaimaiPlayfield.NoteStartDistance*2),
                    RelativePositionAxes = Axes.None,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = HitObject.NoteColor,
                    InnerRadius = .025f,
                    RelativeSizeAxes = Axes.None,
                    Rotation =  -45,
                    Current = new Bindable<double>(0.25),
                    Alpha = 0f,
                },
                note = new Container
                {
                    Scale = Vector2.Zero,
                    Position = new Vector2(0, -26),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    Size = new Vector2(80),
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(1),
                            Child = new CircularContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                BorderThickness = 15,
                                BorderColour = Color4.Crimson,
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                    AlwaysPresent = true,
                                }
                            }
                        },
                        new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            BorderThickness = 3,
                            BorderColour = Color4.Black,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true,
                            }
                        }
                    }
                },
                flash = new FlashPiece
                {
                    Position = new Vector2(0,-MaimaiPlayfield.IntersectDistance),
                },
                explode = new ExplodePiece
                {
                    Position = new Vector2(0,-MaimaiPlayfield.IntersectDistance),
                    Colour = Color4.Crimson,
                    Child = new TrianglesPiece
                    {
                        Blending = BlendingParameters.Additive,
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.5f,
                        Velocity = 5f,
                    }
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

            if (length >= (MaimaiPlayfield.IntersectDistance - 66))
                note.Delay(idle)
                .FadeInFromZero(500)
                .ScaleTo(1f, fadeIn)
                .Then()
                .ResizeHeightTo(MaimaiPlayfield.IntersectDistance - 66 + 80, moveTo)
                .Delay((HitObject as IHasEndTime).Duration)
                .ResizeHeightTo(80, moveTo)
                .MoveToY(-(MaimaiPlayfield.IntersectDistance - 40), moveTo);
            else
                note.Delay(idle)
                .FadeInFromZero(500)
                .ScaleTo(1f, fadeIn)
                .Then()
                .ResizeHeightTo(length + 80, extendTime)
                .Then()
                .MoveToY(-(MaimaiPlayfield.IntersectDistance - length - 80 + 40), AnimationDuration.Value - extendTime)
                .Then()
                .ResizeHeightTo(80, extendTime)
                .MoveToY(-(MaimaiPlayfield.IntersectDistance - 40), extendTime);

            //HitObjectLine.Delay(idle).FadeTo(.75f, fadeIn).Then().ResizeTo(600, moveTo);
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
                    this.FadeTo(1f, 100);
                    explode.FadeTo(1f, 100);
                }
                else
                {
                    this.FadeTo(.5f, 200);
                    explode.FadeTo(0f, 200);
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
                    const double flash_in = 40;
                    const double flash_out = 100;

                    using (BeginDelayedSequence((HitObject as IHasEndTime).Duration, true))
                    {
                        flash.FadeTo(0.8f, flash_in)
                             .Then()
                             .FadeOut(flash_out);

                        explode.FadeIn(flash_in).ScaleTo(1.5f, 400, Easing.OutQuad);
                        HitObjectLine.FadeOut();

                        using (BeginDelayedSequence(flash_in, true))
                        {
                            //after the flash, we can hide some elements that were behind it
                            note.FadeOut();
                            this.FadeOut(800);
                        }
                    }
                    break;
                case ArmedState.Miss:
                    var sequence = note.Delay((HitObject as IHasEndTime).Duration);

                    sequence.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                       .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                       .MoveToOffset(new Vector2(0, -100), time_fade_hit, Easing.OutCubic)
                       .FadeOut(time_fade_miss);

                    sequence.FadeOut(time_fade_miss);
                    HitObjectLine.Delay((HitObject as IHasEndTime).Duration).FadeOut();
                    this.Delay((HitObject as IHasEndTime).Duration).ScaleTo(1f, time_fade_miss).Expire();
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
