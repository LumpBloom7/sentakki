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

        public Container<DrawableSlideCheckpoint> SlideCheckpoints { get; private set; }

        public ISlideVisual Slidepath;
        public Container<StarPiece> SlideStars;

        protected float StarProg;
        public virtual float StarProgress
        {
            get => StarProg;
            set
            {
                StarProg = value;
                foreach (var slideStar in SlideStars)
                {
                    slideStar.Position = ((SlideVisual)Slidepath).Path.PositionAt(value);
                    slideStar.Rotation = ((SlideVisual)Slidepath).Path.PositionAt(value - .01f).GetDegreesFromPosition(((SlideVisual)Slidepath).Path.PositionAt(value + .01f));
                }
            }
        }

        public DrawableSlideBody() : this(null) { }
        public DrawableSlideBody(SlideBody hitObject)
            : base(hitObject) { }

        protected virtual ISlideVisual CreateSlideVisuals() => new SlideVisual();

        protected virtual void CreateSlideStars()
        {
            SlideStars.Add(new StarPiece
            {
                Alpha = 0,
                Scale = Vector2.Zero,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Position = SentakkiExtensions.GetCircularPosition(296.5f, 22.5f),
                RelativeSizeAxes = Axes.None,
            });
        }

        protected virtual void UpdateSlidePath()
        {
            ((SlideVisual)Slidepath).Path = HitObject.SlideInfo.SlidePath;
        }

        [BackgroundDependencyLoader(true)]
        private void load()
        {
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Rotation = -22.5f;
            Slidepath = CreateSlideVisuals();
            AddRangeInternal(new Drawable[]
            {
                (Drawable)Slidepath,
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

            CreateSlideStars();

            AccentColour.BindValueChanged(c => Colour = c.NewValue);
            OnNewResult += queueProgressUpdate;
            OnRevertResult += queueProgressUpdate;
        }

        protected override void OnApply()
        {
            base.OnApply();
            UpdateSlidePath();
            updatePathProgress();
            StarProgress = 0;
        }

        protected override void OnFree()
        {
            base.OnFree();
            Slidepath.Free();
        }

        // We want to ensure that the correct progress is visually shown on screen
        // I don't think that OnRevert of HitObjects is ordered properly
        // So just to make sure, when multiple OnReverts are called, we just queue for a forced update on the visuals
        // This makes sure that we always have the right visuals shown.
        private bool pendingProgressUpdate;

        private void queueProgressUpdate(DrawableHitObject hitObject, JudgementResult result)
        {
            pendingProgressUpdate = true;
        }

        protected override void Update()
        {
            base.Update();
            if (pendingProgressUpdate)
                updatePathProgress();
        }

        // Used to hide and show segments accurately
        private void updatePathProgress()
        {
            float progress = 0;

            for (int i = 0; i < SlideCheckpoints.Count; ++i)
            {
                if (!SlideCheckpoints[i].Result.IsHit)
                    break;

                progress = SlideCheckpoints[i].HitObject.Progress;
            }

            Slidepath.Progress = progress;

            pendingProgressUpdate = false;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            Slidepath.PerformEntryAnimation(AdjustedAnimationDuration);
            using (BeginAbsoluteSequence(HitObject.StartTime - 50, true))
            {
                foreach (var slideStar in SlideStars)
                    slideStar.FadeInFromZero(HitObject.ShootDelay).ScaleTo(1.25f, HitObject.ShootDelay);

                this.Delay(50 + HitObject.ShootDelay).TransformTo(nameof(StarProgress), 1f, (HitObject as IHasDuration).Duration - HitObject.ShootDelay);
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
                    // Miss the last node to ensure that all of them have results
                    SlideCheckpoints[^1].ForcefullyMiss();
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
