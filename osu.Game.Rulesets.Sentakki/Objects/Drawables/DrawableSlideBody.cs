using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideBody : DrawableSentakkiLanedHitObject
    {
        public new SlideBody HitObject => (SlideBody)base.HitObject;

        public override bool RemoveWhenNotAlive => false;

        protected override double InitialLifetimeOffset => base.InitialLifetimeOffset;

        public Container<DrawableSlideCheckpoint> SlideCheckpoints { get; private set; } = null!;

        public SlideVisual Slidepath { get; private set; } = null!;

        public Container<StarPiece> SlideStars { get; private set; } = null!;

        private float starProgress;
        public virtual float StarProgress
        {
            get => starProgress;
            set
            {
                starProgress = value;

                for (int i = 2; i >= 0; --i)
                {
                    int laneOffset = ((i * 2) - 1) % 3;

                    SlideStars[i].Position = Slidepath.Path.PositionAt(value, laneOffset);
                    SlideStars[i].Rotation = Slidepath.Path.PositionAt(value - .01f, laneOffset).GetDegreesFromPosition(Slidepath.Path.PositionAt(value + .01f, laneOffset));

                    if (i != 2 && value < Slidepath.Path.FanStartProgress)
                        break;
                }
            }
        }

        public DrawableSlideBody() : this(null) { }
        public DrawableSlideBody(SlideBody? hitObject)
            : base(hitObject) { }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Rotation = -22.5f;

            AddRangeInternal(new Drawable[]
            {
                Slidepath = new SlideVisual(),
                SlideStars = new Container<StarPiece>{
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                SlideCheckpoints = new Container<DrawableSlideCheckpoint>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            });

            for (int i = 0; i < 3; ++i)
                SlideStars.Add(new StarPiece
                {
                    Alpha = 0,
                    Scale = Vector2.Zero,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Position = SentakkiExtensions.GetCircularPosition(296.5f, 22.5f),
                    RelativeSizeAxes = Axes.None,
                });

            AccentColour.BindValueChanged(c => Colour = c.NewValue);
            OnNewResult += updateSlideCompletion;
            OnRevertResult += updateSlideCompletion;
        }

        protected override void OnApply()
        {
            base.OnApply();
            Slidepath.Path = HitObject.SlideBodyInfo.SlidePath;
            StarProgress = 0;
        }

        protected override void OnFree()
        {
            base.OnFree();
            Slidepath.Free();
        }

        // Updates the path to have correct information of completion progress, then updates the visuals
        private void updateSlideCompletion(DrawableHitObject hitObject, JudgementResult result)
        {
            updateCompletionProgress();
            Slidepath.UpdateChevronVisibility();
        }

        // Used to hide and show segments accurately
        private void updateCompletionProgress()
        {
            float progress = 0;

            for (int i = 0; i < SlideCheckpoints.Count; ++i)
            {
                if (!SlideCheckpoints[i].Result.IsHit)
                    break;

                progress = SlideCheckpoints[i].HitObject.Progress;
            }

            Slidepath.Progress = progress;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            Slidepath.PerformEntryAnimation(AdjustedAnimationDuration);
            using (BeginAbsoluteSequence(HitObject.StartTime - 50))
            {
                SlideStars[2].FadeInFromZero(HitObject.ShootDelay).ScaleTo(1.25f, HitObject.ShootDelay);
                SlideStars[0].FadeOut().ScaleTo(1.25f, HitObject.ShootDelay);
                SlideStars[1].FadeOut().ScaleTo(1.25f, HitObject.ShootDelay);

                if (Slidepath.Path.StartsWithSlideFan)
                {
                    SlideStars[0].FadeInFromZero(HitObject.ShootDelay);
                    SlideStars[1].FadeInFromZero(HitObject.ShootDelay);
                }

                using (BeginDelayedSequence(50 + HitObject.ShootDelay))
                {
                    if (!Slidepath.Path.StartsWithSlideFan && Slidepath.Path.EndsWithSlideFan)
                        using (BeginDelayedSequence((HitObject.Duration - HitObject.ShootDelay) * Slidepath.Path.FanStartProgress))
                        {
                            SlideStars[0].FadeIn();
                            SlideStars[1].FadeIn();
                        }

                    this.TransformTo(nameof(StarProgress), 1f, (HitObject as IHasDuration).Duration - HitObject.ShootDelay);
                }
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            // Player completed all nodes, we consider this user triggered
            userTriggered = true;
            for (int i = 0; i < SlideCheckpoints.Count; ++i)
            {
                if (!SlideCheckpoints[i].Result.HasResult)
                {
                    userTriggered = false;
                    break;
                }
            }

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                {
                    // Ensure that all of them have results
                    foreach (var checkpoint in SlideCheckpoints)
                        checkpoint.ForcefullyMiss();

                    if (SlideCheckpoints.Count(node => !node.Result.IsHit) <= 2 && SlideCheckpoints.Count > 2)
                        ApplyResult(HitResult.Ok);
                    else
                        ApplyResult(Result.Judgement.MinResult);
                }

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                result = HitResult.Ok;

            ApplyResult(result);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);
            const double time_fade_miss = 400 /* time_fade_miss = 400 */;
            switch (state)
            {
                case ArmedState.Hit:
                    using (BeginAbsoluteSequence(Math.Max(Result.TimeAbsolute, HitObject.GetEndTime() - HitObject.HitWindows.WindowFor(HitResult.Good))))
                    {
                        Slidepath.PerformExitAnimation(200);

                        foreach (var star in SlideStars)
                            star.FadeOut(200);

                        this.FadeOut(200).Expire();
                    }

                    break;
                case ArmedState.Miss:
                    Slidepath.PerformExitAnimation(time_fade_miss);
                    this.FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint).FadeOut(time_fade_miss).Expire();
                    break;
            }
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case SlideCheckpoint checkpoint:
                    return new DrawableSlideCheckpoint(checkpoint)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoBindable = { BindTarget = AutoBindable },
                    };
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);
            switch (hitObject)
            {
                case DrawableSlideCheckpoint node:
                    SlideCheckpoints.Add(node);
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            SlideCheckpoints.Clear(false);
        }
    }
}
