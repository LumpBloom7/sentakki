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
    public class DrawableTouch : DrawableSentakkiHitObject, IDrawableHitObjectWithProxiedApproach
    {
        // IsHovered is used
        public override bool HandlePositionalInput => true;

        public Drawable ProxiedLayer => this;

        protected override float SamplePlaybackPosition => (HitObject.Position.X + SentakkiPlayfield.INTERSECTDISTANCE) / (SentakkiPlayfield.INTERSECTDISTANCE * 2);

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

        public DrawableTouch(SentakkiHitObject hitObject) : base(hitObject)
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
        private readonly DefaultEasingFunction inOutBack = new DefaultEasingFunction(Easing.InOutBack);
        private readonly DefaultEasingFunction inOutSine = new DefaultEasingFunction(Easing.InOutSine);

        protected override void Update()
        {
            base.Update();
            if (Result.HasResult) return;

            double fadeIn = AnimationDuration.Value * GameplaySpeed;
            double moveTo = HitObject.HitWindows.WindowFor(HitResult.Meh) * 2 * GameplaySpeed;
            double animStart = HitObject.StartTime - fadeIn - moveTo;
            double currentProg = Clock.CurrentTime - animStart;

            // Calculate initial entry animation
            float fadeAmount = Math.Clamp((float)(currentProg / fadeIn), 0, 1);

            Alpha = fadeAmount * (float)inOutBack.ApplyEasing(fadeAmount);
            Scale = new Vector2(fadeAmount * (float)inOutBack.ApplyEasing(fadeAmount));

            // Calculate position
            float moveAmount = Math.Clamp((float)((currentProg - fadeIn) / moveTo), 0, 1);

            // Used to simplify this crazy arse manual animating
            float moveAnimFormula(float originalValue) => (float)(originalValue - (originalValue * inOutSine.ApplyEasing(moveAmount)));

            blob1.Position = new Vector2(moveAnimFormula(40), 0);
            blob2.Position = new Vector2(moveAnimFormula(-40), 0);
            blob3.Position = new Vector2(0, moveAnimFormula(40));
            blob4.Position = new Vector2(0, moveAnimFormula(-40));

            // Used to simplify this crazy arse manual animating
            float sizeAnimFormula() => (float)(.5 + .5 * inOutSine.ApplyEasing(moveAmount));

            blob1.Scale = new Vector2(sizeAnimFormula());
            blob2.Scale = new Vector2(sizeAnimFormula());
            blob3.Scale = new Vector2(sizeAnimFormula());
            blob4.Scale = new Vector2(sizeAnimFormula());

            // Might be literally jank, but it removes bad edges after animation finishes
            if (moveAmount == 1)
            {
                blob2.Alpha = 0;
                blob3.Alpha = 0;
                blob4.Alpha = 0;
            }
            else
            {
                blob2.Alpha = 1;
                blob3.Alpha = 1;
                blob4.Alpha = 1;
            }


            // Handle hidden and fadeIn modifications
            if (IsHidden)
            {
                float hideAmount = Math.Clamp((float)((currentProg - fadeIn) / (moveTo / 2)), 0, 1);

                Alpha = 1 - hideAmount;
            }
            else if (IsFadeIn)
            {
                // Using existing moveAmount because it serves our needs
                Alpha = 1 * moveAmount;
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
