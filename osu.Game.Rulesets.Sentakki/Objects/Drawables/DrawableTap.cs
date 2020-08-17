using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;
using System;
using System.Diagnostics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableTap : DrawableSentakkiHitObject
    {
        public readonly HitReceptor HitArea;
        public readonly Drawable TapVisual;
        public readonly HitObjectLine HitObjectLine;
        protected override double InitialLifetimeOffset => 8000;

        public DrawableTap(SentakkiHitObject hitObject)
            : base(hitObject)
        {
            AccentColour.Value = hitObject.NoteColor;
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            AlwaysPresent = true;
            AddRangeInternal(new Drawable[] {
                HitObjectLine = new HitObjectLine(),
                TapVisual = CreateTapRepresentation(),
                HitArea = new HitReceptor()
                {
                    Hit = () =>
                    {
                        if (AllJudged)
                            return false;

                        UpdateResult(true);
                        return true;
                    },
                    Position = new Vector2(0, -SentakkiPlayfield.INTERSECTDISTANCE),
                },
            });
        }

        protected virtual Drawable CreateTapRepresentation() => new TapPiece();

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager settings)
        {
            settings?.BindWith(SentakkiRulesetSettings.AnimationDuration, AnimationDuration);
            AccentColour.BindValueChanged(c => HitObjectLine.Colour = c.NewValue, true);
        }

        protected override void UpdateInitialTransforms()
        {
            double animTime = AnimationDuration.Value / 2 * GameplaySpeed;
            double animStart = HitObject.StartTime - (animTime * 2);
            using (BeginAbsoluteSequence(animStart, true))
            {
                TapVisual.FadeInFromZero(animTime).ScaleTo(1, animTime);
                HitObjectLine.FadeInFromZero(animTime);
                using (BeginDelayedSequence(animTime, true))
                {
                    var excessDistance = (-SentakkiPlayfield.INTERSECTDISTANCE + SentakkiPlayfield.NOTESTARTDISTANCE) / animTime * HitObject.HitWindows.WindowFor(HitResult.Miss);
                    TapVisual.MoveToY((float)(-SentakkiPlayfield.INTERSECTDISTANCE + excessDistance), animTime + HitObject.HitWindows.WindowFor(HitResult.Miss));
                    HitObjectLine.ScaleTo(1, animTime);
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
                {
                    ApplyResult(r => r.Type = HitResult.Miss);
                }

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None || (result == HitResult.Miss && Time.Current < HitObject.StartTime))
                return;

            ApplyResult(r => r.Type = result);
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);
            const double time_fade_hit = 400, time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    this.Delay(400).FadeOut().Expire();
                    HitObjectLine.FadeOut();

                    break;

                case ArmedState.Miss:
                    TapVisual.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                       .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                       .MoveToOffset(new Vector2(0, -100), time_fade_hit, Easing.OutCubic)
                       .FadeOut(time_fade_miss);
                    HitObjectLine.FadeOut();

                    this.ScaleTo(1f, time_fade_miss).Expire();

                    break;
            }
        }
    }
}
