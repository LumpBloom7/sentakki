using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.UI;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables;

public partial class DrawableSlideCheckpoint : DrawableSentakkiHitObject
{
    // Slides parts can be hit as long as the body is visible, regardless of it's intended time
    // By setting the animation duration to an absurdly high value, the lifetimes of touch regions are bounded by the parent DrawableSlide.
    protected override double InitialLifetimeOffset => double.MaxValue;

    public new SlideCheckpoint HitObject => (SlideCheckpoint)base.HitObject;

    public override bool DisplayResult => false;

    private DrawableSlideBody parentHitObject => (DrawableSlideBody)ParentHitObject;
    private int slideOriginLane => parentHitObject.ParentHitObject.HitObject.Lane;

    // Used to determine the node order
    public int ThisIndex;

    // Hits are only possible if this the second node before this one is hit
    // If the second node before this one doesn't exist, it is allowed as this is one of the first nodes
    // All hits can only be done after the slide tap has been judged
    public bool IsHittable => parentHitObject.IsHittable && isPreviousNodeHit();

    public bool StrictSliderTracking { get; set; }

    // Short slides should always have strict tracking
    // This is a QoL improvement that prevents short slides from being completed without intention when hitting a laned note along the tail.
    private bool shouldBeStrict => StrictSliderTracking || parentHitObject.SlideCheckpoints.Count <= 3;

    private int trackingLookBehindDistance => shouldBeStrict ? 1 : 2;

    private bool isPreviousNodeHit() => ThisIndex < trackingLookBehindDistance || parentHitObject.SlideCheckpoints[ThisIndex - trackingLookBehindDistance].IsHit;

    private Container<DrawableSlideCheckpointNode> nodes = null!;

    protected override float SamplePlaybackPosition =>
        SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, slideOriginLane).X / (SentakkiPlayfield.INTERSECTDISTANCE * 2) + .5f;

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
        ThisIndex = parentHitObject.SlideCheckpoints.Count;
    }

    protected override void CheckForResult(bool userTriggered, double timeOffset)
    {
        // Counting hit notes manually to avoid LINQ alloc overhead
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

        // The previous node may not be judged due to slider hit order leniency allowing players to skip up to one node
        if (ThisIndex > 0)
            parentHitObject.SlideCheckpoints[ThisIndex - 1].ApplyResult(result);

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
