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
        public readonly TapCircle CirclePiece;
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
                CirclePiece = new TapCircle()
                {
                    Scale = new Vector2(0f),
                    Position = new Vector2(0, -SentakkiPlayfield.NOTESTARTDISTANCE)
                },
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
            hitObject.PathBindable.BindValueChanged(r =>
            {
                Rotation = r.NewValue.GetAngleFromPath();
                HitArea.NotePath = r.NewValue;
            }, true);
        }

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager settings)
        {
            settings?.BindWith(SentakkiRulesetSettings.AnimationDuration, AnimationDuration);
            HitObjectLine.Colour = HitObject.NoteColor;
        }

        protected override void Update()
        {
            base.Update();
            if (Result.HasResult) return;

            double animTime = AnimationDuration.Value / 2 * GameplaySpeed;
            double animStart = HitObject.StartTime - (animTime * 2);
            double currentProg = Clock.CurrentTime - animStart;

            // Calculate initial entry animation
            float fadeAmount = Math.Clamp((float)(currentProg / animTime), 0, 1);

            CirclePiece.Alpha = fadeAmount;
            CirclePiece.Scale = new Vector2(fadeAmount);
            HitObjectLine.Alpha = fadeAmount;

            // Calculate position
            float moveAmount = Math.Clamp((float)((currentProg - animTime) / animTime), 0, 1);
            CirclePiece.Y = (float)Interpolation.Lerp(-SentakkiPlayfield.NOTESTARTDISTANCE, -SentakkiPlayfield.INTERSECTDISTANCE, moveAmount);

            // Handle hidden and fadeIn modifications
            if (IsHidden)
            {
                float hideAmount = Math.Clamp((float)((currentProg - animTime) / (animTime / 2)), 0, 1);
                Alpha = 1 - hideAmount;
            }
            else if (IsFadeIn)
            {
                // Using existing moveAmount because it serves our needs
                Alpha = 1 * moveAmount;
            }

            HitObjectLine.UpdateVisual(moveAmount);
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
            HitObjectLine.FadeOut();

            switch (state)
            {
                case ArmedState.Hit:
                    this.Delay(400).FadeOut().Expire();

                    break;

                case ArmedState.Miss:

                    CirclePiece.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                       .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                       .MoveToOffset(new Vector2(0, -100), time_fade_hit, Easing.OutCubic)
                       .FadeOut(time_fade_miss);

                    this.ScaleTo(1f, time_fade_miss).Expire();

                    break;
            }
        }
    }
}
