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
    public class DrawableTouch : DrawableSentakkiHitObject
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
            Size = new Vector2(240);
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
                },
                new HitReceptor{
                    Hit = () =>
                    {
                        if (AllJudged)
                            return false;

                        UpdateResult(true);
                        return false;
                    },
                }
            });
        }

        protected override void UpdateInitialTransforms()
        {
            double largestHitWindow = HitObject.HitWindows.WindowFor(HitResult.Meh);
            this.FadeIn(500, Easing.InOutBack).ScaleTo(1, 500, Easing.InOutBack);

            using (BeginDelayedSequence(1000 - largestHitWindow, true))
            {
                circle1.MoveTo(Vector2.Zero, largestHitWindow).ResizeTo(80, largestHitWindow);
                circle2.MoveTo(Vector2.Zero, largestHitWindow).ResizeTo(80, largestHitWindow);
                circle3.MoveTo(Vector2.Zero, largestHitWindow).ResizeTo(80, largestHitWindow);
                circle4.MoveTo(Vector2.Zero, largestHitWindow).ResizeTo(80, largestHitWindow);
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (Auto && timeOffset > 0)
                    ApplyResult(r => r.Type = HitResult.Perfect);

                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = HitResult.Miss);

                return;
            }
            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (timeOffset < 0 && result <= HitResult.Miss)
                return;
            if (result >= HitResult.Meh && timeOffset < 0)
                result = HitResult.Perfect;

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
    }
}