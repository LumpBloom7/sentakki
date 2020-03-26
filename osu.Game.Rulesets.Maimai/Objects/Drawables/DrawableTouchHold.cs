// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets.Maimai.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Graphics;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;
using System.Linq;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables
{
    public class DrawableTouchHold : DrawableMaimaiHitObject
    {
        private readonly CircularProgress progress;
        private readonly TouchHoldCircle circle;
        private readonly CircularContainer ring;
        private readonly SpriteText text;
        private readonly FlashPiece flash;
        private readonly ExplodePiece explode;
        private readonly GlowPiece glow;
        private readonly CircularContainer outline;

        public override bool HandlePositionalInput => true;

        private MaimaiInputManager maimaiActionInputManager;
        internal MaimaiInputManager MaimaiActionInputManager => maimaiActionInputManager ??= GetContainingInputManager() as MaimaiInputManager;

        protected override double InitialLifetimeOffset => 500;

        public DrawableTouchHold(TouchHold hitObject)
            : base(hitObject)
        {
            AccentColour.Value = Color4.HotPink;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(120);
            Scale = new Vector2(0f);
            RelativeSizeAxes = Axes.None;
            Alpha = 0;
            AddRangeInternal(new Drawable[] {
                glow = new GlowPiece(){
                    Alpha = 0f,
                    Colour = AccentColour.Value,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Padding = new MarginPadding(1),
                    Children = new Drawable[]
                    {
                        ring = new Circle
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                        },
                        new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = Vector2.One,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            Child = progress = new CircularProgress
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                InnerRadius = 0.250f,
                                Size = Vector2.One,
                                RelativeSizeAxes = Axes.Both,
                                Colour = AccentColour.Value,
                                Current = { Value = 0 },
                            }
                        },
                    }
                },
                outline = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderColour = Color4.Black,
                    BorderThickness = 3,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true,
                    }
                },
                circle = new TouchHoldCircle
                {

                    Colour = AccentColour.Value,
                    Size = new Vector2(0.77f),
                    RelativeSizeAxes = Axes.Both,
                },
                text = new SpriteText
                {
                    Text = "HOLD!",
                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 32),
                    Colour = Color4.White,
                    ShadowColour = Color4.Black,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                flash = new FlashPiece(),
                explode = new ExplodePiece{
                    Colour = AccentColour.Value
                },
            });
        }

        protected override void UpdateInitialTransforms()
        {
            this.FadeTo(.5f, 500).ScaleTo(.8f, 500);
            progress.Delay(500).FillTo(1f, (HitObject as TouchHold).Duration);
        }

        private double potential = 0;
        private double held = 0;
        private bool buttonHeld = false;
        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current < HitObject.StartTime) return;

            if (userTriggered || Time.Current < (HitObject as IHasEndTime)?.EndTime)
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
                else if (Time.Current >= (HitObject as IHasEndTime)?.EndTime)
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
                    held++;
                    this.Alpha = 1f;
                    this.ScaleTo(1f, 100);
                    glow.FadeTo(1f, 100);
                }
                else
                {
                    this.Alpha = .5f;
                    //this.Scale = new Vector2(.9f);
                    this.ScaleTo(.9f, 200);
                    glow.FadeTo(0f, 200);
                }
                base.Update();
            }
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            switch (state)
            {
                case ArmedState.Hit:
                    const double flash_in = 40;
                    const double flash_out = 100;

                    flash.Delay((HitObject as IHasEndTime).Duration).FadeTo(0.8f, flash_in)
                         .Then()
                         .FadeOut(flash_out);

                    explode.Delay((HitObject as IHasEndTime).Duration).FadeIn(flash_in);
                    this.Delay((HitObject as IHasEndTime).Duration).ScaleTo(1.5f, 400, Easing.OutQuad);

                    using (BeginDelayedSequence((HitObject as IHasEndTime).Duration + flash_in, true))
                    {
                        outline.FadeOut();
                        progress.FadeOut();
                        text.FadeOut();
                        ring.FadeOut();
                        circle.FadeOut();
                        this.FadeOut(800);
                    }
                    break;
                case ArmedState.Miss:
                    this.Delay((HitObject as IHasEndTime).Duration).ScaleTo(.0f, 400).FadeOut(400);
                    break;
            }
        }
    }
}
