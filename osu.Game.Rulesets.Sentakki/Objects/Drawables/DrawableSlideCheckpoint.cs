using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideCheckpoint : DrawableSentakkiHitObject
    {
        public new SlideCheckpoint HitObject => (SlideCheckpoint)base.HitObject;

        // We need this to be alive as soon as the parent slide note is alive
        // This is to ensure reverts are still possible during edge case situation (eg. 0 duration slide)
        protected override bool ShouldBeAlive => Time.Current < LifetimeEnd;

        public override bool DisplayResult => false;

        private IDrawableSlideBody parentSlide => (IDrawableSlideBody)ParentHitObject;

        // Used to determine the node order
        public int ThisIndex;

        // Hits are only possible if this the second node before this one is hit
        // If the second node before this one doesn't exist, it is allowed as this is one of the first nodes
        // All hits can only be done after the parent StartTime
        public bool IsHittable => Time.Current > ParentHitObject.HitObject.StartTime && (ThisIndex < 2 || parentSlide.SlideCheckpoints[ThisIndex - 2].IsHit);

        private Container<DrawableSlideCheckpointNode> nodes;

        public DrawableSlideCheckpoint() : this(null) { }
        public DrawableSlideCheckpoint(SlideCheckpoint checkpoint)
            : base(checkpoint) { }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            AddInternal(nodes = new Container<DrawableSlideCheckpointNode>()
            {
                RelativeSizeAxes = Axes.Both
            });
        }

        protected override void OnApply()
        {
            base.OnApply();

            // Nodes are applied before being added to the parent playfield, so this node isn't in SlideNodes yet
            // Since we know that the node isn't in the container yet, and that the count is always one higher than the topmost element, we can use that as the predicted index
            ThisIndex = parentSlide.SlideCheckpoints.Count;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            // Don't allow for user input if auto is enabled for touch based objects (AutoTouch mod)
            if (nodes.Count(n => n.IsHit) >= HitObject.NodesToPass)
                ApplyResult(Result.Judgement.MaxResult);
        }

        protected override void ApplyResult(Action<JudgementResult> application)
        {
            // Judge the previous node, because that isn't guaranteed due to the leniency;
            if (ThisIndex > 0)
                parentSlide.SlideCheckpoints[ThisIndex - 1]?.ApplyResult(application);

            // Make sure remaining nodes are judged
            foreach (var node in nodes)
                node.ApplyResult(application);

            base.ApplyResult(application);
        }

        // Forcefully miss this node, used when players fail to complete the slide on time.
        public void ForcefullyMiss() => ApplyResult(r => r.Type = r.Judgement.MinResult);

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case SlideCheckpoint.CheckpointNode node:
                    return new DrawableSlideCheckpointNode(node)
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
                case DrawableSlideCheckpointNode node:
                    nodes.Add(node);
                    break;
            }
        }
        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            nodes.Clear(false);
        }
    }
}
