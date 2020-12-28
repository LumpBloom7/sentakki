using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideBody : DrawableSentakkiLanedHitObject
    {
        public new SlideBody HitObject => (SlideBody)base.HitObject;

        public override bool RemoveWhenNotAlive => false;

        protected override double InitialLifetimeOffset => base.InitialLifetimeOffset;

        public Container<DrawableSlideNode> SlideNodes;

        public SlideVisual Slidepath;
        public StarPiece SlideStar;

        private float starProg;
        public float StarProgress
        {
            get => starProg;
            set
            {
                starProg = value;
                SlideStar.Position = Slidepath.Path.PositionAt(value);
                SlideStar.Rotation = Slidepath.Path.PositionAt(value - .01f).GetDegreesFromPosition(Slidepath.Path.PositionAt(value + .01f));
            }
        }

        public DrawableSlideBody() : this(null) { }
        public DrawableSlideBody(SlideBody hitObject)
            : base(hitObject) { }

        [BackgroundDependencyLoader(true)]
        private void load()
        {
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Rotation = -22.5f;
            AddRangeInternal(new Drawable[]
            {
                Slidepath = new SlideVisual(),
                new Container{
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = SlideStar = new StarPiece
                    {
                        Alpha = 0,
                        Scale = Vector2.Zero,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Position = SentakkiExtensions.GetCircularPosition(296.5f,22.5f),
                        RelativeSizeAxes  = Axes.None,
                    }
                },
                SlideNodes = new Container<DrawableSlideNode>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            });

            AccentColour.BindValueChanged(c => Colour = c.NewValue);
            OnNewResult += queueProgressUpdate;
            OnRevertResult += queueProgressUpdate;
        }

        protected override void OnApply()
        {
            base.OnApply();
            Slidepath.Path = HitObject.SlideInfo.SlidePath.Path;
            updatePathProgress();
            StarProgress = 0;

            AccentColour.BindTo(ParentHitObject.AccentColour);
        }

        protected override void OnFree()
        {
            base.OnFree();
            AccentColour.UnbindFrom(ParentHitObject.AccentColour);
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
            var target = SlideNodes.LastOrDefault(x => x.Result.IsHit);
            if (target == null)
                Slidepath.CompletedSegments = 0;
            else
                Slidepath.CompletedSegments = target.ThisIndex + 1;

            pendingProgressUpdate = false;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            Slidepath.PerformEntryAnimation(AdjustedAnimationDuration);
            using (BeginAbsoluteSequence(HitObject.StartTime - 50, true))
            {
                SlideStar.FadeInFromZero(HitObject.ShootDelay).ScaleTo(1.25f, HitObject.ShootDelay);
                this.Delay(50 + HitObject.ShootDelay).TransformTo(nameof(StarProgress), 1f, (HitObject as IHasDuration).Duration - HitObject.ShootDelay);
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            // Player completed all nodes, we consider this user triggered
            if (SlideNodes.All(node => node.Result.HasResult))
                userTriggered = true;

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                {
                    // Miss the last node to ensure that all of them have results
                    SlideNodes.Last().ForcefullyMiss();
                    if (SlideNodes.Count(node => !node.Result.IsHit) <= 2 && SlideNodes.Count > 2)
                        ApplyResult(r => r.Type = HitResult.Meh);
                    else
                        ApplyResult(r => r.Type = r.Judgement.MinResult);
                }

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                result = HitResult.Meh;

            ApplyResult(r => r.Type = result);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);
            const double time_fade_miss = 400 /* time_fade_miss = 400 */;
            switch (state)
            {
                case ArmedState.Hit:
                    using (BeginAbsoluteSequence(Math.Max(Result.TimeAbsolute, HitObject.GetEndTime() - HitObject.HitWindows.WindowFor(HitResult.Great))))
                    {
                        Slidepath.PerformExitAnimation(200);
                        SlideStar.FadeOut(200);
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
                case SlideBody.SlideNode node:
                    return new DrawableSlideNode(node)
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
                case DrawableSlideNode node:
                    SlideNodes.Add(node);
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            SlideNodes.Clear(false);
        }
    }
}
