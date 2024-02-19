using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableSlideCheckpoint : DrawableSentakkiHitObject
    {
        public new SlideCheckpoint HitObject => (SlideCheckpoint)base.HitObject;

        // We need this to be alive as soon as the parent slide note is alive
        // This is to ensure reverts are still possible during edge case situation (eg. 0 duration slide)
        protected override bool ShouldBeAlive => Time.Current < LifetimeEnd;

        public override bool DisplayResult => false;

        private new DrawableSlideBody ParentHitObject => (DrawableSlideBody)base.ParentHitObject;

        // Used to determine the node order
        public int ThisIndex;

        // Hits are only possible if this the second node before this one is hit
        // If the second node before this one doesn't exist, it is allowed as this is one of the first nodes
        // All hits can only be done after the slide tap has been judged
        public bool IsHittable => ParentHitObject.IsHittable && isPreviousNodeHit();

        private bool isPreviousNodeHit() => ThisIndex < 1 || ParentHitObject.SlideCheckpoints[ThisIndex - 1].IsHit;

        private Container<DrawableSlideCheckpointNode> nodes = null!;

        public DrawableSlideCheckpoint()
            : this(null)
        {
        }

        public DrawableSlideCheckpoint(SlideCheckpoint? checkpoint)
            : base(checkpoint)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            AddInternal(nodes = new Container<DrawableSlideCheckpointNode>
            {
                RelativeSizeAxes = Axes.Both
            });
        }

        protected override void OnApply()
        {
            base.OnApply();

            // Nodes are applied before being added to the parent playfield, so this node isn't in SlideNodes yet
            // Since we know that the node isn't in the container yet, and that the count is always one higher than the topmost element, we can use that as the predicted index
            ThisIndex = ParentHitObject.SlideCheckpoints.Count;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            // Counting  hit notes manually to avoid LINQ alloc overhead
            int hitNotes = 0;

            foreach (var node in nodes)
            {
                if (node.IsHit)
                    ++hitNotes;
            }

            if (hitNotes >= HitObject.NodesToPass)
                ApplyResult(Result.Judgement.MaxResult);
        }

        protected new void ApplyResult(HitResult result)
        {
            if (Judged)
                return;

            // Make sure remaining nodes are judged
            foreach (var node in nodes)
                node.ApplyResult(result);

            base.ApplyResult(result);
        }

        // Forcefully miss this node, used when players fail to complete the slide on time.
        public void ForcefullyMiss() => ApplyResult(Result.Judgement.MinResult);

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
