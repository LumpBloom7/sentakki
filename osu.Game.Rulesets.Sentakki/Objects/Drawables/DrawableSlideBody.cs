using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK;
using osuTK.Graphics;
using System.Linq;
using System.Diagnostics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideBody : DrawableSentakkiHitObject
    {
        public override bool RemoveWhenNotAlive => false;

        public override bool DisplayResult => true;

        public Container<DrawableSlideNode> SlideNodes;

        public SlideVisual Slidepath;
        public StarPiece SlideStar;

        protected override double InitialLifetimeOffset => 1000 + (HitObject as IHasDuration).Duration;

        private float starProg = 0;
        private Vector2? previousPosition = null;
        public float StarProgress
        {
            get => starProg;
            set
            {
                starProg = value;
                SlideStar.Position = Slidepath.Path.PositionAt(value);
                if (previousPosition == null)
                    SlideStar.Rotation = SlideStar.Position.GetDegreesFromPosition(Slidepath.Path.PositionAt(value + .001f));
                else
                    SlideStar.Rotation = previousPosition.Value.GetDegreesFromPosition(SlideStar.Position);
                previousPosition = SlideStar.Position;
            }
        }

        public DrawableSlideBody(SentakkiHitObject hitObject)
            : base(hitObject)
        {
            AccentColour.BindTo(HitObject.ColourBindable);
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Rotation = -22.5f;
            AddRangeInternal(new Drawable[]
            {
                Slidepath = new SlideVisual
                {
                    Alpha = 0,
                    Path = (hitObject as SlideBody).SlideInfo.SlidePath.Path,
                },
                new Container{
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = SlideStar = new StarPiece
                    {
                        Alpha = 0,
                        Scale = Vector2.Zero,
                        Position = Slidepath.Path.PositionAt(0),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes  = Axes.None,
                        Size = new Vector2(75),
                    }
                },
                SlideNodes = new Container<DrawableSlideNode>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            });

            AccentColour.BindValueChanged(c => Colour = c.NewValue, true);

            OnNewResult += queueProgressUpdate;
            OnRevertResult += queueProgressUpdate;
        }

        [Resolved]
        private Bindable<WorkingBeatmap> workingBeatmap { get; set; }

        public double ShootDelay
        {
            get
            {
                double delay = workingBeatmap.Value.Beatmap.ControlPointInfo.TimingPointAt(HitObject.StartTime).BeatLength * (HitObject as SlideBody).SlideInfo.ShootDelay / 2;
                if (delay >= (HitObject as IHasDuration).Duration - 50)
                    return 0;
                return delay;
            }
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

        private void updatePathProgress()
        {
            var target = SlideNodes.LastOrDefault(x => x.Result.IsHit);
            if (target == null)
                Slidepath.Progress = 0;
            else Slidepath.Progress = (target.HitObject as SlideBody.SlideNode).Progress;

            pendingProgressUpdate = false;
        }

        protected override void UpdateInitialTransforms()
        {
            using (BeginAbsoluteSequence(HitObject.StartTime - 500, true))
            {
                Slidepath.FadeInFromZero(500);
                using (BeginAbsoluteSequence(HitObject.StartTime - 50, true))
                {
                    SlideStar.FadeInFromZero(100).ScaleTo(1, 100);
                    this.Delay(100 + ShootDelay).TransformTo(nameof(StarProgress), 1f, (HitObject as IHasDuration).Duration - 50 - ShootDelay);
                }
            }
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);
            const double time_fade_miss = 400 /* time_fade_miss = 400 */;
            switch (state)
            {
                case ArmedState.Hit:
                    this.FadeOut();
                    break;
                case ArmedState.Miss:
                    using (BeginDelayedSequence((HitObject as IHasDuration).Duration))
                    {
                        this.FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint).FadeOut(time_fade_miss).Expire();
                    }
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            SlideNodes.Clear();
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case SlideBody.SlideNode node:
                    node.StartTime = HitObject.StartTime + ShootDelay + (((HitObject as IHasDuration).Duration - ShootDelay) * node.Progress);
                    return new DrawableSlideNode(node, this)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AccentColour = { BindTarget = AccentColour },
                        AutoBindable = { BindTarget = AutoBindable },
                        ThisIndex = SlideNodes.Count
                    };
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableSlideNode node:
                    SlideNodes.Add(node);
                    break;
            }
            base.AddNestedHitObject(hitObject);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);
            if (SlideNodes.All(node => node.Result.HasResult))
                userTriggered = true;

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                {
                    SlideNodes.Last().ForcefullyMiss();
                    if (SlideNodes.Count(node => !node.Result.IsHit) <= 2 && SlideNodes.Count > 2)
                        ApplyResult(r => r.Type = HitResult.Meh);
                    else
                        ApplyResult(r => r.Type = HitResult.Miss);
                }

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                result = HitResult.Meh;

            ApplyResult(r => r.Type = result);
        }
    }
}
