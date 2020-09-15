using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;
using System.Diagnostics;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Game.Rulesets.Sentakki.Configuration;
using System;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableTouch : DrawableSentakkiHitObject
    {
        // IsHovered is used
        public override bool HandlePositionalInput => true;

        protected override float SamplePlaybackPosition => ((HitObject as Touch).Position.X / (SentakkiPlayfield.INTERSECTDISTANCE * 2)) + .5f;

        protected override double InitialLifetimeOffset => 6000;

        private readonly TouchBlob blob1;
        private readonly TouchBlob blob2;
        private readonly TouchBlob blob3;
        private readonly TouchBlob blob4;

        private readonly TouchFlashPiece flash;
        private readonly ExplodePiece explode;

        private readonly CircularContainer dot;

        public readonly HitReceptor HitArea;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        public DrawableTouch(Touch hitObject) : base(hitObject)
        {
            Size = new Vector2(80);
            Position = hitObject.Position;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Alpha = 0;
            Scale = Vector2.Zero;
            Colour = HitObject.NoteColor;
            AlwaysPresent = true;
            AddRangeInternal(new Drawable[]{
                blob1 = new TouchBlob{
                    Position = new Vector2(40, 0)
                },
                blob2 = new TouchBlob{
                    Position = new Vector2(-40, 0)
                },
                blob3 = new TouchBlob{
                    Position = new Vector2(0, 40)
                },
                blob4 = new TouchBlob{
                    Position = new Vector2(0, -40)
                },
                dot = new CircularContainer
                {
                    Size = new Vector2(20),
                    Masking = true,
                    BorderColour = Color4.Gray,
                    BorderThickness = 2,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        AlwaysPresent = true,
                        Colour = Color4.White,
                    }
                },
                flash = new TouchFlashPiece(),
                explode = new ExplodePiece(),
                HitArea = new HitReceptor{
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

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfigs)
        {
            sentakkiConfigs?.BindWith(SentakkiRulesetSettings.TouchAnimationDuration, AnimationDuration);
        }

        // Easing functions for manual use.
        private readonly DefaultEasingFunction outSine = new DefaultEasingFunction(Easing.OutSine);
        private readonly DefaultEasingFunction inQuint = new DefaultEasingFunction(Easing.InQuint);

        protected override void UpdateInitialTransforms()
        {
            double fadeIn = AnimationDuration.Value * GameplaySpeed;
            double moveTo = HitObject.HitWindows.WindowFor(HitResult.Meh) * 2 * GameplaySpeed;

            using (BeginAbsoluteSequence(HitObject.StartTime - fadeIn - moveTo, true))
            {
                this.FadeIn(fadeIn, Easing.OutSine).ScaleTo(1, fadeIn, Easing.OutSine);

                using (BeginDelayedSequence(fadeIn, true))
                {
                    blob1.MoveTo(new Vector2(0), moveTo, Easing.InQuint).ScaleTo(1, moveTo, Easing.InQuint);
                    blob2.MoveTo(new Vector2(0), moveTo, Easing.InQuint).ScaleTo(1, moveTo, Easing.InQuint).Then().FadeOut();
                    blob3.MoveTo(new Vector2(0), moveTo, Easing.InQuint).ScaleTo(1, moveTo, Easing.InQuint).Then().FadeOut();
                    blob4.MoveTo(new Vector2(0), moveTo, Easing.InQuint).ScaleTo(1, moveTo, Easing.InQuint).Then().FadeOut();
                }
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

            if (result == HitResult.None)
                return;

            if (timeOffset < 0)
            {
                if (result <= HitResult.Miss) return;

                if (result < HitResult.Great) result = HitResult.Great;
            }

            ApplyResult(r => r.Type = result);
        }

        protected override void InvalidateTransforms()
        {
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

                    flash.FadeTo(0.8f, flash_in)
                         .Then()
                         .FadeOut(flash_out);

                    dot.Delay(flash_in).FadeOut();

                    explode.FadeIn(flash_in);
                    this.ScaleTo(1.5f, 400, Easing.OutQuad);

                    this.Delay(time_fade_hit).FadeOut().Expire();

                    break;

                case ArmedState.Miss:
                    this.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                       .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                       .FadeOut(time_fade_miss);
                    break;
            }
        }
    }
}
