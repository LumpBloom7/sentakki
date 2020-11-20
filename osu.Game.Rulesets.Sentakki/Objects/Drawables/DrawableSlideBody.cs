using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideBody : DrawableSentakkiLanedHitObject
    {
        public new SlideBody HitObject => (SlideBody)base.HitObject;
        public override bool RemoveWhenNotAlive => false;

        public override bool DisplayResult => true;

        public Container<DrawableSlideNode> SlideNodes;

        public SlideVisual Slidepath;
        public StarPiece SlideStar;

        private float starProg;
        private Vector2? previousPosition;
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

        public DrawableSlideBody() : this(null) { }

        public DrawableSlideBody(SlideBody hitObject)
            : base(hitObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Rotation = -22.5f;
            AddRangeInternal(new Drawable[]
            {
                Slidepath = new SlideVisual
                {
                    Alpha = 0,
                },
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
                        Size = new Vector2(75),
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

        protected override void OnApply(HitObject hitObject)
        {
            base.OnApply(hitObject);
            Slidepath.Path = ((SlideBody)hitObject).SlideInfo.SlidePath.Path;
            updatePathProgress();
            previousPosition = null;
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

        protected override double InitialLifetimeOffset => base.InitialLifetimeOffset / 2;

        protected override void UpdateInitialTransforms()
        {
            Slidepath.FadeInFromZero(AdjustedAnimationDuration / 2);
            using (BeginAbsoluteSequence(HitObject.StartTime - 50, true))
            {
                SlideStar.FadeInFromZero(100).ScaleTo(1, 100);
                this.Delay(100 + HitObject.ShootDelay).TransformTo(nameof(StarProgress), 1f, (HitObject as IHasDuration).Duration - 50 - HitObject.ShootDelay);
            }
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);
            const double time_fade_miss = 400 /* time_fade_miss = 400 */;
            switch (state)
            {
                case ArmedState.Hit:
                    SlideStar.FadeOut();
                    this.Delay(2000).FadeOut(); // Hack to make sure sounds stay alive
                    break;
                case ArmedState.Miss:
                    this.FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint).FadeOut(time_fade_miss).Expire();
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
