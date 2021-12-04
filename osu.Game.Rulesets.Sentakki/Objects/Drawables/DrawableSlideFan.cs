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
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideFan : DrawableSentakkiLanedHitObject, IDrawableSlideBody
    {
        public new SlideFan HitObject => (SlideFan)base.HitObject;

        public override bool RemoveWhenNotAlive => false;

        protected override double InitialLifetimeOffset => base.InitialLifetimeOffset;

        public Container<DrawableSlideCheckpoint> SlideCheckpoints { get; private set; }
        public Container<DrawableSlideTap> SlideTaps;

        public SlideFanVisual Slidepath;

        public Container<StarPiece> SlideStars;

        public DrawableSlideFan() : this(null) { }
        public DrawableSlideFan(SlideFan hitObject)
            : base(hitObject) { }

        [BackgroundDependencyLoader(true)]
        private void load()
        {
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            AddRangeInternal(new Drawable[]
            {
                Slidepath = new SlideFanVisual(), // This needs to be updated
                // We need three stars instead of 1
                SlideStars = new Container<StarPiece>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                // Probably needs a specialized node
                SlideCheckpoints = new Container<DrawableSlideCheckpoint>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                SlideTaps = new Container<DrawableSlideTap>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });

            for (int i = 0; i < 3; ++i)
                SlideStars.Add(new StarPiece
                {
                    Y = -SentakkiPlayfield.INTERSECTDISTANCE,
                    Alpha = 0,
                    Scale = Vector2.Zero,
                    // Each node is 22.5 degrees apart, when measured from a node's perspective.
                    Rotation = 157.5f + (22.5f * i),
                });

            AccentColour.BindValueChanged(c => Colour = c.NewValue);
            OnNewResult += queueProgressUpdate;
            OnRevertResult += queueProgressUpdate;
        }

        protected override void OnApply()
        {
            base.OnApply();
            updatePathProgress();
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
                for (int i = 0; i < SlideStars.Count; ++i)
                {
                    Vector2 destination = SentakkiExtensions.GetCircularPosition(SentakkiPlayfield.INTERSECTDISTANCE, SentakkiPlayfield.LANEANGLES[3 + i] - 22.5f);
                    SlideStars[i].FadeInFromZero(HitObject.ShootDelayAbsolute)
                                 .ScaleTo(1.25f, HitObject.ShootDelayAbsolute)
                                 .Then()
                                 .Delay(50)
                                 .MoveTo(destination, (HitObject as IHasDuration).Duration - HitObject.ShootDelayAbsolute);
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
                    // Miss the last node to ensure that all of them have results
                    //SlideNodes.Last().ForcefullyMiss();
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
                case SlideTap x:
                    return new DrawableSlideTap(x)
                    {
                        AutoBindable = { BindTarget = AutoBindable },
                    };
                case SlideCheckpoint node:
                    return new DrawableSlideCheckpoint(node)
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
            switch (hitObject)
            {
                case DrawableSlideTap tap:
                    SlideTaps.Child = tap;
                    break;
                case DrawableSlideCheckpoint node:
                    SlideCheckpoints.Add(node);
                    break;
            }
            base.AddNestedHitObject(hitObject);
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            SlideCheckpoints.Clear(false);
            SlideTaps.Clear(false);
        }
    }
}
