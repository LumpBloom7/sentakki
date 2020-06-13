using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;
using System.Linq;
using osu.Framework.Graphics.Effects;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableTouchHold : DrawableSentakkiHitObject
    {
        private readonly TouchHoldCircle circle;

        public override bool HandlePositionalInput => true;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        protected override double InitialLifetimeOffset => 4000;

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
            AlwaysPresent = true;
            AddRangeInternal(new Drawable[] {
                circle = new TouchHoldCircle(){ Duration = hitObject.Duration },
            });
        }

        private double timeHeld = 0;
        private bool buttonHeld = false;

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current < HitObject.StartTime) return;

            if (userTriggered || Time.Current < (HitObject as IHasDuration)?.EndTime)
                return;

            FinishTransforms(true);
            double result = timeHeld / (HitObject as IHasDuration).Duration;

            ApplyResult(r =>
            {
                if (result >= .9)
                    r.Type = HitResult.Perfect;
                else if (result >= .75)
                    r.Type = HitResult.Great;
                else if (result >= .5)
                    r.Type = HitResult.Good;
                else if (result >= .25)
                    r.Type = HitResult.Ok;
                else if (Time.Current >= (HitObject as IHasDuration)?.EndTime)
                    r.Type = HitResult.Miss;
            });
        }

        private readonly Bindable<double> touchAnimationDuration = new Bindable<double>(1000);

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfigs)
        {
            sentakkiConfigs?.BindWith(SentakkiRulesetSettings.TouchAnimationDuration, touchAnimationDuration);
        }

        [Resolved]
        private OsuColour colours { get; set; }
        private Color4 currentColour
        {
            get => AccentColour.Value;
            set => AccentColour.Value = value;
        }

        protected override void Update()
        {
            base.Update();
            if (Result.HasResult) return;

            double fadeIn = touchAnimationDuration.Value * GameplaySpeed;
            double animStart = HitObject.StartTime - fadeIn;
            double currentProg = Clock.CurrentTime - animStart;

            // Calculate initial entry animation
            float fadeAmount = (float)(currentProg / fadeIn);
            if (fadeAmount < 0) fadeAmount = 0;
            else if (fadeAmount > 1) fadeAmount = 1;

            Alpha = fadeAmount;
            Scale = new Vector2(1f * fadeAmount);

            // Calculate progressbar fill
            float fillAmount = (float)((currentProg - fadeIn) / (HitObject as TouchHold).Duration);
            if (fillAmount < 0) fillAmount = 0;
            else if (fillAmount > 1) fillAmount = 1;

            circle.Progress.Current.Value = fillAmount;

            // Hidden fade calculation
            float hiddenAmount = (float)((currentProg - fadeIn) / 125);
            if (hiddenAmount < 0) hiddenAmount = 0;
            else if (hiddenAmount > 1) hiddenAmount = 1;

            if (IsHidden && hiddenAmount > 0)
                Alpha = 1 - (1 * hiddenAmount / ((Time.Current >= HitObject.StartTime && ((buttonHeld && IsHovered) || Auto)) ? 2 : 1));

            // Input and feedback
            if (Time.Current >= HitObject.StartTime && Time.Current <= (HitObject as IHasDuration)?.EndTime)
            {
                bool activated = (SentakkiActionInputManager?.PressedActions.Any() ?? false) && IsHovered;
                if (activated || Auto)
                {
                    float amount = 1f;
                    timeHeld += Clock.ElapsedFrameTime;
                    double progress = timeHeld / (HitObject as IHasDuration).Duration;
                    Color4 newColor = Color4.HotPink;
                    var newEdge = circle.GlowEdgeEffect.Value;
                    if (progress > .25f)
                    {
                        amount += .033f;
                        newColor = colours.ForHitResult(HitResult.Meh);
                        newEdge.Colour = colours.ForHitResult(HitResult.Meh);
                        newEdge.Radius = 25;
                    }

                    if (progress > .5f)
                    {
                        amount += .033f;
                        newColor = colours.ForHitResult(HitResult.Good);
                        newEdge.Colour = colours.ForHitResult(HitResult.Good);
                        newEdge.Radius = 35;
                    }
                    if (progress > .75f)
                    {
                        amount += .034f;
                        newColor = colours.ForHitResult(HitResult.Great);
                        newEdge.Colour = colours.ForHitResult(HitResult.Great);
                        newEdge.Radius = 45;
                    }
                    circle.GlowEdgeEffect.Value = newEdge;

                    this.TransformTo(nameof(currentColour), newColor, 100);
                    circle.FadeTo(amount, 100);
                    circle.ScaleTo(amount, 100);
                }
                else
                {
                    circle.FadeTo(.5f, 100);
                    circle.ScaleTo(.8f, 200);
                }
                base.Update();
            }
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);
            const double time_fade_hit = 400, time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    this.Delay((HitObject as IHasDuration).Duration).ScaleTo(1f, time_fade_hit).Expire();
                    break;

                case ArmedState.Miss:
                    this.Delay((HitObject as IHasDuration).Duration).ScaleTo(.0f, time_fade_miss).FadeOut(time_fade_miss);
                    break;
            }
        }
    }
}
