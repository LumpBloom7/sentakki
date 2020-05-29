using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;
using System;
using System.Diagnostics;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableTouch : DrawableSentakkiHitObject, IKeyBindingHandler<SentakkiAction>
    {
        // IsHovered is used
        public override bool HandlePositionalInput => true;

        protected override float SamplePlaybackPosition => (HitObject.Position.X + SentakkiPlayfield.INTERSECTDISTANCE) / (SentakkiPlayfield.INTERSECTDISTANCE * 2);

        protected override double InitialLifetimeOffset => 1000;

        private readonly CircularContainer circle1;
        private readonly CircularContainer circle2;
        private readonly CircularContainer circle3;
        private readonly CircularContainer circle4;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        public DrawableTouch(SentakkiHitObject hitObject) : base(hitObject)
        {
            Size = new Vector2(80);
            Position = hitObject.Position;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Alpha = 0;
            Scale = Vector2.Zero;
            AlwaysPresent = true;
            AddRangeInternal(new Drawable[]{
                circle1 = new CircularContainer{
                    Masking = true,
                    Position = new Vector2(40, 0),
                    Size = new Vector2(40),
                    BorderColour = Color4.Red,
                    Origin = Anchor.Centre,
                    Anchor =Anchor.Centre,
                    BorderThickness = 3,
                    Child = new Box{
                        RelativeSizeAxes = Axes.Both,
                        Alpha= .2f,
                        AlwaysPresent = true,
                        Colour = Color4.Red,
                    }
                },
                circle2 = new CircularContainer{
                    Masking = true,
                    Position = new Vector2(-40, 0),
                    Size = new Vector2(40),
                    BorderColour = Color4.Red,
                    Origin = Anchor.Centre,
                    Anchor =Anchor.Centre,
                    BorderThickness = 3,
                    Child = new Box{
                        RelativeSizeAxes = Axes.Both,
                        Alpha= .2f,
                        AlwaysPresent = true,
                        Colour = Color4.Red,
                    }
                },
                circle3 = new CircularContainer{
                    Masking = true,
                    Position = new Vector2(0, 40),
                    Size = new Vector2(40),
                    BorderColour = Color4.Red,
                    Origin = Anchor.Centre,
                    Anchor =Anchor.Centre,
                    BorderThickness = 3,
                    Child = new Box{
                        RelativeSizeAxes = Axes.Both,
                        Alpha= .2f,
                        AlwaysPresent = true,
                        Colour = Color4.Red,
                    }
                },
                circle4 = new CircularContainer{
                    Masking = true,
                    Position = new Vector2(0, -40),
                    Size = new Vector2(40),
                    BorderColour = Color4.Red,
                    Origin = Anchor.Centre,
                    Anchor =Anchor.Centre,
                    BorderThickness = 3,
                    Child = new Box{
                        RelativeSizeAxes = Axes.Both,
                        Alpha= .2f,
                        AlwaysPresent = true,
                        Colour = Color4.Red,
                    }
                }
            });
        }

        protected override void UpdateInitialTransforms()
        {
            this.FadeIn(500, Easing.InOutBack).ScaleTo(1, 500, Easing.InOutBack);
            circle1.Delay(500).MoveTo(Vector2.Zero, 500, Easing.InOutBack).ResizeTo(80, 500, Easing.InOutBack);
            circle2.Delay(500).MoveTo(Vector2.Zero, 500, Easing.InOutBack).ResizeTo(80, 500, Easing.InOutBack);
            circle3.Delay(500).MoveTo(Vector2.Zero, 500, Easing.InOutBack).ResizeTo(80, 500, Easing.InOutBack);
            circle4.Delay(500).MoveTo(Vector2.Zero, 500, Easing.InOutBack).ResizeTo(80, 500, Easing.InOutBack);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (Auto && timeOffset > 0)
                    ApplyResult(r => r.Type = HitResult.Perfect);

                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                {
                    ApplyResult(r => r.Type = HitResult.Miss);
                }

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (Time.Current < HitObject.StartTime && (result == HitResult.Good || result == HitResult.Miss)) return;
            if (result == HitResult.None || (result == HitResult.Miss && Time.Current < HitObject.StartTime))
                return;

            ApplyResult(r => r.Type = result);
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            switch (state)
            {
                case ArmedState.Hit:
                    this.ScaleTo(1.5f, 200).FadeOut(200).Then().Expire();

                    break;

                case ArmedState.Miss:
                    this.ScaleTo(0, 400).FadeOut(400).Then().Expire();

                    break;
            }
        }

        public virtual bool OnPressed(SentakkiAction action)
        {
            if (AllJudged || !IsHovered)
                return false;

            UpdateResult(true);
            return true;
        }

        public void OnReleased(SentakkiAction action)
        {
        }
        protected override bool OnHover(HoverEvent e)
        {
            if (!SentakkiActionInputManager.PressedActions.Any()) return false;
            if (AllJudged)
                return false;

            UpdateResult(true);
            return true;
        }

    }
}