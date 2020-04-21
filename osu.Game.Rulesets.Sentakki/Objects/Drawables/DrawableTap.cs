// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;
using System;
using System.Diagnostics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableTap : DrawableSentakkiHitObject
    {
        public readonly HitReceptor HitArea;
        public readonly TapCircle CirclePiece;
        public readonly CircularContainer HitObjectLine;

        private double fadeIn = 500, moveTo, idle;

        protected override double InitialLifetimeOffset => 3500;

        public DrawableTap(SentakkiHitObject hitObject)
            : base(hitObject)
        {
            AccentColour.Value = hitObject.NoteColor;
            RelativeSizeAxes = Axes.Both;
            CornerRadius = 120;
            CornerExponent = 2;
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            AddRangeInternal(new Drawable[] {
                HitObjectLine = new HitObjectLine
                {
                    Rotation = HitObject.Angle,
                },
                CirclePiece = new TapCircle()
                {
                    Scale = new Vector2(0f),
                    Rotation = hitObject.Angle,
                    Position = HitObject.Position
                },
                HitArea = new HitReceptor()
                {
                    Hit = () =>
                    {
                        if (AllJudged)
                            return false;

                        UpdateResult(true);
                        return true;
                    },
                    RelativeSizeAxes = Axes.None,
                    Position = hitObject.endPosition
                },
            });
        }

        private Bindable<double> animationDuration = new Bindable<double>(1000);

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager settings)
        {
            settings?.BindWith(SentakkiRulesetSettings.AnimationDuration, animationDuration);
            HitObjectLine.Child.Colour = HitObject.NoteColor;
        }

        protected override void UpdateInitialTransforms()
        {
            animationDuration.TriggerChange();
            fadeIn = 500;
            moveTo = animationDuration.Value;
            idle = 3500 - fadeIn - moveTo;
            base.UpdateInitialTransforms();

            CirclePiece.Delay(idle).FadeInFromZero(fadeIn).ScaleTo(1f, fadeIn).Then().MoveTo(HitObject.endPosition, moveTo);
            HitObjectLine.Delay(idle).Then(h => h.FadeTo(.75f, fadeIn).Then(h => h.ResizeTo(600, moveTo)));
            if (IsHidden)
                this.Delay(idle + fadeIn).FadeOut(moveTo / 2);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                {
                    ApplyResult(r => r.Type = HitResult.Miss);
                }

                return;
            }

            if (HitObject.HitWindows.ResultFor(timeOffset) == HitResult.Miss && Time.Current < HitObject.StartTime) return;

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
            {
                return;
            }

            ApplyResult(r => r.Type = result);
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            const double time_fade_hit = 400, time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    var b = HitObject.Angle + 90;
                    var a = b * (float)(Math.PI / 180);

                    HitObjectLine.FadeOut();
                    this.ScaleTo(1f, time_fade_hit).Expire();

                    break;

                case ArmedState.Miss:
                    var c = HitObject.Angle + 90;
                    var d = c * (float)(Math.PI / 180);

                    CirclePiece.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                       .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                       .MoveToOffset(new Vector2(-(100 * (float)Math.Cos(d)), -(100 * (float)Math.Sin(d))), time_fade_hit, Easing.OutCubic)
                       .FadeOut(time_fade_miss);

                    HitObjectLine.FadeOut();
                    this.ScaleTo(1f, time_fade_miss).Expire();

                    break;
            }
        }

        public class HitReceptor : CircularContainer, IKeyBindingHandler<SentakkiAction>
        {
            // IsHovered is used
            public override bool HandlePositionalInput => true;

            public Func<bool> Hit;

            public SentakkiAction? HitAction;

            private SentakkiInputManager sentakkiActionInputManager;
            internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

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
                            HitAction = action;
                            return true;
                        }
                        break;
                }

                return false;
            }

            public void OnReleased(SentakkiAction action)
            {
            }
        }
    }
}
