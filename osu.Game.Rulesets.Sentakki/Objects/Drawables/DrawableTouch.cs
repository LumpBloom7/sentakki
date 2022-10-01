using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Skinning;
using osu.Game.Rulesets.Sentakki.Skinning.Default.Touches;
using osu.Game.Skinning;
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
        protected override double InitialLifetimeOffset => base.InitialLifetimeOffset + HitObject.HitWindows.WindowFor(HitResult.Ok);

        // Similar to IsHovered for mouse, this tracks whether a pointer (touch or mouse) is interacting with this drawable
        // Interaction == (IsHovered && ActionPressed) || (OnTouch && TouchPointerInBounds)
        public bool[] PointInteractionState = new bool[11];
        public SkinnableDrawable TouchBody = null!;

        private SentakkiInputManager sentakkiActionInputManager = null!;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= (SentakkiInputManager)GetContainingInputManager();

        public DrawableTouch() : this(null) { }
        public DrawableTouch(Touch? hitObject)
            : base(hitObject) { }

        [BackgroundDependencyLoader]
        private void load(SentakkiRulesetConfigManager? sentakkiConfigs)
        {
            sentakkiConfigs?.BindWith(SentakkiRulesetSettings.TouchAnimationDuration, AnimationDuration);

            Size = new Vector2(105);
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            AddRangeInternal(new Drawable[]{
                TouchBody = new SkinnableDrawable(new SentakkiSkinComponent(SentakkiSkinComponents.Touch), _ => new TouchBody())
                {
                    Size = new Vector2(130),
                    Alpha = 0,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                },
            });

            trackedKeys.BindValueChanged(x =>
            {
                if (AllJudged)
                    return;

                UpdateResult(true);
            });
        }

        protected override void OnApply()
        {
            base.OnApply();
            Position = HitObject.Position;
        }

        protected override void OnFree()
        {
            base.OnFree();
            for (int i = 0; i < 11; ++i)
                PointInteractionState[i] = false;
        }

        private BindableInt trackedKeys = new BindableInt(0);

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            double FadeIn = AdjustedAnimationDuration / 2;
            double moveTo = HitObject.HitWindows.WindowFor(HitResult.Ok);

            TouchBody.FadeIn(FadeIn);

            using (BeginDelayedSequence(AdjustedAnimationDuration, true))
            {
                TouchBody.ResizeTo(90, moveTo, Easing.InCirc);
                ((TouchBody)TouchBody.Drawable).BorderContainer.Delay(moveTo).FadeIn();
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            // Don't allow for user input if auto is enabled for touch based objects (AutoTouch mod)
            if (!userTriggered || Auto)
            {
                if (Auto && timeOffset > 0)
                    ApplyResult(Result.Judgement.MaxResult);
                else if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(Result.Judgement.MinResult);

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);

            if (result == HitResult.None)
                return;

            if (timeOffset < 0)
                result = Result.Judgement.MaxResult;

            ApplyResult(result);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);
            const double time_fade_hit = 100, time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    TouchBody.FadeOut();
                    this.Delay(time_fade_hit).Expire();

                    break;

                case ArmedState.Miss:
                    TouchBody.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                       .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                       .FadeOut(time_fade_miss);

                    this.Delay(time_fade_miss).Expire();
                    break;
            }
        }

        public bool OnNewPointInteraction() => UpdateResult(true);
    }
}
