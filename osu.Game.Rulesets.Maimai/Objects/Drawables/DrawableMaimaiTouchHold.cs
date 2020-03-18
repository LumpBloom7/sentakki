// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets.Maimai.Configuration;
using osu.Game.Rulesets.Maimai.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;
using System.Linq;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables
{
    public class DrawableMaimaiTouchHold : DrawableMaimaiHitObject
    {
        public CircularProgress progress;
        public Circle circle;
        public SpriteText text;

        private readonly FlashPiece flash;
        private readonly ExplodePiece explode;

        private Bindable<Color4> accentColor;
        double fadeIn = 500;
        private MaimaiTouchHold hitObject_;
        public override bool HandlePositionalInput => true;

        private MaimaiInputManager maimaiActionInputManager;
        internal MaimaiInputManager MaimaiActionInputManager => maimaiActionInputManager ??= GetContainingInputManager() as MaimaiInputManager;

        protected override double InitialLifetimeOffset => 500;

        public DrawableMaimaiTouchHold(MaimaiTouchHold hitObject)
            : base(hitObject)
        {
            hitObject_ = hitObject;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(150);
            Scale = new Vector2(0f);
            RelativeSizeAxes = Axes.None;
            AddRangeInternal(new Drawable[] {
                progress = new CircularProgress
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    InnerRadius = 1f,
                    Size = new Vector2(150),
                    Colour = Color4.Purple,
                    Current = { Value = 0 },
                },
                circle = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    Size = new Vector2(100),
                    Colour = Color4.Azure,
                },
                text = new SpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "Hold!",
                    Colour = Color4.Black,
                },
                flash = new FlashPiece(),
                explode = new ExplodePiece(),
            });
        }

        protected override void UpdateInitialTransforms()
        {
            this.FadeInFromZero(500).ScaleTo(1f, 500);
            progress.ScaleTo(1f, 500).Then().FillTo(1f, hitObject_.Duration - 400f);
        }

        private double potential = 0;
        private double held = 0;
        private bool buttonHeld = false;
        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current < HitObject.StartTime) return;

            if (userTriggered || Time.Current < (HitObject as IHasEndTime)?.EndTime - 400f)
                return;

            double result = held / potential;

            ApplyResult(r =>
            {
                if (result >= 1)
                    r.Type = HitResult.Perfect;
                else if (result >= .9)
                    r.Type = HitResult.Great;
                else if (result >= .8)
                    r.Type = HitResult.Good;
                else if (result >= .5)
                    r.Type = HitResult.Ok;
                else if (Time.Current >= (HitObject as IHasEndTime)?.EndTime - 400f)
                    r.Type = HitResult.Miss;
            });
        }

        public bool Auto = false;
        protected override void Update()
        {
            buttonHeld = MaimaiActionInputManager?.PressedActions.Any(x => x == MaimaiAction.Button1 || x == MaimaiAction.Button2) ?? false;
            if (Time.Current >= HitObject.StartTime && Time.Current <= (HitObject as IHasEndTime)?.EndTime)
            {
                potential++;
                if ((buttonHeld && IsHovered) || Auto)
                {
                    held++; text.Text = (held / potential).FormatAccuracy();
                    this.Alpha = 1f;
                }
                else
                {
                    text.Text = "Hold!";
                    this.Alpha = .5f;
                }
                base.Update();
            }
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            const double time_fade_hit = 400, time_fade_miss = 400;

            var sequence = this.Delay((HitObject as IHasEndTime).Duration);

            switch (state)
            {
                case ArmedState.Hit:
                    const double flash_in = 40;
                    const double flash_out = 100;

                    flash.Delay((HitObject as IHasEndTime).Duration).FadeTo(0.8f, flash_in)
                         .Then()
                         .FadeOut(flash_out);

                    explode.Delay((HitObject as IHasEndTime).Duration).FadeIn(flash_in);
                    progress.Delay((HitObject as IHasEndTime).Duration).FadeOut();
                    text.Delay((HitObject as IHasEndTime).Duration).FadeOut();
                    this.Delay((HitObject as IHasEndTime).Duration).ScaleTo(1.5f, 400, Easing.OutQuad);

                    using (BeginDelayedSequence(flash_in, true))
                    {
                        //after the flash, we can hide some elements that were behind it
                        circle.Delay((HitObject as IHasEndTime).Duration).FadeOut();

                        this.Delay((HitObject as IHasEndTime).Duration).FadeOut(800);
                    }
                    break;
                case ArmedState.Miss:
                    sequence.ScaleTo(.0f, time_fade_miss);
                    break;
            }
        }
    }
}
