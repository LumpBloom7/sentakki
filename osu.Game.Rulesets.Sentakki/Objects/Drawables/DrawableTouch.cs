using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableTouch : DrawableSentakkiHitObject
    {
        protected new Touch HitObject => (Touch)base.HitObject;

        // IsHovered is used
        public override bool HandlePositionalInput => true;

        // This HitObject uses a completely different offset
        protected override double InitialLifetimeOffset => base.InitialLifetimeOffset + HitObject.HitWindows.WindowFor(HitResult.Meh);

        private TouchFlashPiece flash;
        private ExplodePiece explode;
        private TouchBody touchBody;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        public DrawableTouch() : this(null) { }
        public DrawableTouch(Touch hitObject)
            : base(hitObject) { }

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfigs)
        {
            sentakkiConfigs?.BindWith(SentakkiRulesetSettings.TouchAnimationDuration, AnimationDuration);

            Size = new Vector2(130);
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Alpha = 0;
            AddRangeInternal(new Drawable[]{
                touchBody = new TouchBody(),
                flash = new TouchFlashPiece{
                    RelativeSizeAxes = Axes.None,
                    Size = new Vector2(105)
                },
                explode = new ExplodePiece{
                    RelativeSizeAxes = Axes.None,
                    Size = new Vector2(105)
                },
            });

            trackedKeys.BindValueChanged(x =>
            {
                if (AllJudged)
                    return;

                UpdateResult(true);
            });

            AccentColour.BindValueChanged(c =>
            {
                flash.Colour = c.NewValue;
                explode.Colour = c.NewValue;
            }, true);
        }

        protected override void OnApply()
        {
            base.OnApply();
            Position = HitObject.Position;
        }

        private BindableInt trackedKeys = new BindableInt(0);

        protected override void Update()
        {
            Position = HitObject.Position;
            base.Update();
            int count = 0;

            if (!AllJudged)
            {
                var touchInput = SentakkiActionInputManager.CurrentState.Touch;
                if (touchInput.ActiveSources.Any())
                {
                    foreach (var t in touchInput.ActiveSources)
                        if (ReceivePositionalInputAt(touchInput.GetTouchPosition(t).Value)) ++count;
                }
                else if (IsHovered)
                {
                    foreach (var a in SentakkiActionInputManager.PressedActions)
                        if (a < SentakkiAction.Key1) ++count;
                }
            }

            trackedKeys.Value = count;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            double FadeIn = AdjustedAnimationDuration / 2;
            double moveTo = HitObject.HitWindows.WindowFor(HitResult.Meh);

            this.FadeInFromZero(FadeIn);

            using (BeginDelayedSequence(AdjustedAnimationDuration, true))
            {
                touchBody.ResizeTo(90, moveTo, Easing.InCirc);
                touchBody.BorderContainer.Delay(moveTo).FadeIn();
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            // Don't allow for user input if auto is enabled for touch based objects (AutoTouch mod)
            if (!userTriggered || Auto)
            {
                if (Auto && timeOffset > 0)
                    ApplyResult(r => r.Type = r.Judgement.MaxResult);
                else if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = r.Judgement.MinResult);

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);

            if (result == HitResult.None)
                return;

            if (timeOffset < 0)
                result = Result.Judgement.MaxResult;

            ApplyResult(r => r.Type = result);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);
            const double time_fade_hit = 400, time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    const double flash_in = 40;
                    const double flash_out = 100;

                    flash.FadeTo(0.8f, flash_in)
                         .Then()
                         .FadeOut(flash_out);

                    explode.FadeIn(flash_in);
                    touchBody.FadeOut();
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
