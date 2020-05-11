// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;
using System;
using System.Diagnostics;
using osu.Game.Rulesets.Sentakki.UI;
namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableTap : DrawableSentakkiHitObject
    {
        public readonly HitReceptor HitArea;
        public readonly TapCircle CirclePiece;
        public readonly CircularContainer HitObjectLine;
        protected override double InitialLifetimeOffset => 6000;

        public DrawableTap(SentakkiHitObject hitObject)
            : base(hitObject)
        {
            AccentColour.Value = hitObject.NoteColor;
            RelativeSizeAxes = Axes.Both;
            CornerRadius = 120;
            CornerExponent = 2;
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            AlwaysPresent = true;
            AddRangeInternal(new Drawable[] {
                HitObjectLine = new HitObjectLine
                {
                    Rotation = HitObject.Angle,
                },
                CirclePiece = new TapCircle()
                {
                    Scale = new Vector2(0f),
                    Rotation = hitObject.Angle,
                    Position = HitObject.Position
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
                    Position = hitObject.EndPosition,
                    NoteAngle = HitObject.Angle
                },
            });
        }

        private Bindable<double> animationDuration = new Bindable<double>(1000);
        private Bindable<Track> speedAdjustmentTrack = new Bindable<Track>(new TrackVirtual(0));
        private double speed => speedAdjustmentTrack.Value.AggregateTempo.Value * speedAdjustmentTrack.Value.AggregateFrequency.Value;

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager settings, DrawableSentakkiRuleset drawableRuleset)
        {
            settings?.BindWith(SentakkiRulesetSettings.AnimationDuration, animationDuration);
            HitObjectLine.Child.Colour = HitObject.NoteColor;

            speedAdjustmentTrack.BindTo(drawableRuleset.SpeedAdjustmentTrack);
        }

        protected override void Update()
        {
            base.Update();
            if (Result.HasResult) return;
            double fadeIn = 500 * speed;
            double moveTo = animationDuration.Value * speed;
            double animStart = HitObject.StartTime - moveTo - fadeIn;
            double currentProg = Clock.CurrentTime - animStart;

            // Calculate initial entry animation
            float fadeAmount = (float)(currentProg / fadeIn);
            if (fadeAmount < 0) fadeAmount = 0;
            else if (fadeAmount > 1) fadeAmount = 1;

            CirclePiece.Alpha = (float)(1 * fadeAmount);
            CirclePiece.Scale = new Vector2((float)(1 * fadeAmount));
            HitObjectLine.Alpha = (float)(.75 * fadeAmount);

            // Calculate position
            Vector2 positionDifference = HitObject.EndPosition - HitObject.Position;
            float moveAmount = (float)((currentProg - fadeIn) / moveTo);
            if (moveAmount < 0) moveAmount = 0;
            else if (moveAmount > 1) moveAmount = 1;

            CirclePiece.Position = HitObject.Position + (positionDifference * moveAmount);

            // Handle hidden and fadeIn modifications
            if (IsHidden)
            {
                float hideAmount = (float)((currentProg - fadeIn) / (moveTo / 2));
                if (hideAmount < 0) hideAmount = 0;
                else if (hideAmount > 1) hideAmount = 1;

                Alpha = 1 - (1 * hideAmount);
            }
            else if (IsFadeIn)
            {
                // Using existing moveAmount because it serves our needs
                Alpha = 1 * moveAmount;
            }

            // Make sure HitObjectLine is adjusted
            float sizeDiff = 600 - (SentakkiPlayfield.NOTESTARTDISTANCE * 2);
            HitObjectLine.Size = new Vector2((SentakkiPlayfield.NOTESTARTDISTANCE * 2) + (sizeDiff * moveAmount));
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
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
                    var c = HitObject.Angle + 90;
                    var d = c * (float)(Math.PI / 180);

                    CirclePiece.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                       .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                       .MoveToOffset(new Vector2(-(100 * (float)Math.Cos(d)), -(100 * (float)Math.Sin(d))), time_fade_hit, Easing.OutCubic)
                       .FadeOut(time_fade_miss);

                    this.ScaleTo(1f, time_fade_miss).Expire();

                    break;
            }
        }
    }
}
