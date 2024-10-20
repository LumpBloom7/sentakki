using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableTap : DrawableSentakkiLanedHitObject, IKeyBindingHandler<SentakkiAction>
    {
        protected virtual Drawable CreateTapRepresentation() => new TapPiece();

        public override double LifetimeStart
        {
            get => base.LifetimeStart;
            set
            {
                base.LifetimeStart = value;
                TapVisual.LifetimeStart = value;
            }
        }

        public override double LifetimeEnd
        {
            get => base.LifetimeEnd;
            set
            {
                base.LifetimeEnd = value;
                TapVisual.LifetimeEnd = value;
            }
        }

        public Drawable TapVisual = null!;

        public DrawableTap()
            : this(null)
        {
        }

        public DrawableTap(Tap? hitObject = null)
            : base(hitObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            AddRangeInternal(new[]
            {
                TapVisual = CreateTapRepresentation(),
            });
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            double animTime = AnimationDuration.Value / 2;
            TapVisual.FadeInFromZero(animTime).ScaleTo(1, animTime);

            using (BeginDelayedSequence(animTime))
            {
                double excessDistance = (-SentakkiPlayfield.INTERSECTDISTANCE + SentakkiPlayfield.NOTESTARTDISTANCE) / animTime * HitObject.HitWindows.WindowFor(HitResult.Miss);
                TapVisual.MoveToY((float)(-SentakkiPlayfield.INTERSECTDISTANCE + excessDistance), animTime + HitObject.HitWindows.WindowFor(HitResult.Miss));
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
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

            if (result < HitResult.Perfect && HitObject.Ex && result.IsHit())
                result = HitResult.Great;

            ApplyResult(result);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);
            double time_fade_miss = 400 * (DrawableSentakkiRuleset?.GameplaySpeed ?? 1);

            switch (state)
            {
                case ArmedState.Hit:
                    TapVisual.FadeOut();
                    Expire();
                    break;

                case ArmedState.Miss:
                    TapVisual.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                             .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                             .MoveToOffset(new Vector2(0, -100), time_fade_miss, Easing.OutCubic)
                             .FadeOut(time_fade_miss);

                    this.ScaleTo(1f, time_fade_miss).Expire();

                    break;
            }
        }

        public bool OnPressed(KeyBindingPressEvent<SentakkiAction> e)
        {
            if (e.Action != SentakkiAction.Key1 + HitObject.Lane)
                return false;

            return UpdateResult(true);
        }

        public void OnReleased(KeyBindingReleaseEvent<SentakkiAction> e) { }
    }
}
