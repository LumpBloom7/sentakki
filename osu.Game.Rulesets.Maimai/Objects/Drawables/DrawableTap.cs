// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Maimai.Configuration;
using osu.Game.Rulesets.Maimai.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Maimai.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;
using System;
using System.Diagnostics;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables
{
    public class DrawableTap : DrawableMaimaiHitObject
    {
        public readonly HitReceptor HitArea;
        public readonly TapCircle CirclePiece;
        public readonly CircularProgress HitObjectLine;

        private Bindable<Color4> accentColor;
        double fadeIn = 500, moveTo, idle;

        public MaimaiAction? HitAction => HitArea.HitAction;

        protected override double InitialLifetimeOffset => 3500;

        public DrawableTap(MaimaiHitObject hitObject)
            : base(hitObject)
        {
            accentColor = new Bindable<Color4>(hitObject.NoteColor);
            AccentColour.BindTo(accentColor);
            RelativeSizeAxes = Axes.Both;
            CornerRadius = 120;
            CornerExponent = 2;
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            AddRangeInternal(new Drawable[] {
                HitObjectLine = new CircularProgress
                {
                    Size = new Vector2(MaimaiPlayfield.NoteStartDistance*2),
                    RelativePositionAxes = Axes.None,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = HitObject.NoteColor,
                    InnerRadius = .025f,
                    RelativeSizeAxes = Axes.None,
                    Rotation =  -45 +HitObject.Angle,
                    Current = new Bindable<double>(0.25),
                    Alpha = 0f,
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
        Bindable<double> AnimationDuration = new Bindable<double>(1000);

        [BackgroundDependencyLoader(true)]
        private void load(MaimaiRulesetConfigManager settings)
        {
            settings?.BindWith(MaimaiRulesetSettings.AnimationDuration, AnimationDuration);
        }

        protected override void UpdateInitialTransforms()
        {
            AnimationDuration.TriggerChange();
            fadeIn = 500;
            moveTo = AnimationDuration.Value;
            idle = 3500 - fadeIn - moveTo;
            base.UpdateInitialTransforms();

            CirclePiece.Delay(idle).FadeInFromZero(fadeIn).ScaleTo(1f, fadeIn).Then().MoveTo(HitObject.endPosition, moveTo);
            HitObjectLine.Delay(idle).Then(h => h.FadeTo(.75f, fadeIn).Then(h => h.ResizeTo(600, moveTo)));
            if (isHidden)
                using (BeginDelayedSequence(idle + fadeIn))
                {
                    this.FadeOut(moveTo / 2);
                }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = HitResult.Miss);

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None || CheckValidation?.Invoke(this) == false)
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

                    CirclePiece.FadeOut(time_fade_miss);
                    HitObjectLine.FadeOut();
                    this.ScaleTo(1f, time_fade_miss).Expire();

                    break;
            }
        }
        public class HitReceptor : CircularContainer, IKeyBindingHandler<MaimaiAction>
        {
            // IsHovered is used
            public override bool HandlePositionalInput => true;

            public Func<bool> Hit;

            public MaimaiAction? HitAction;
            public HitReceptor()
            {
                RelativeSizeAxes = Axes.None;
                Size = new Vector2(350f);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            public bool OnPressed(MaimaiAction action)
            {
                switch (action)
                {
                    case MaimaiAction.Button1:
                    case MaimaiAction.Button2:
                        if (IsHovered && (Hit?.Invoke() ?? false))
                        {
                            HitAction = action;
                            return true;
                        }
                        break;
                }

                return false;
            }
            public void OnReleased(MaimaiAction action)
            {
            }
        }
    }
}
