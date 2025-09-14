using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables;

public partial class DrawableSlideCheckpointNode : DrawableSentakkiHitObject
{
    // Slides parts can be hit as long as the body is visible, regardless of it's intended time
    // By setting the animation duration to an absurdly high value, the lifetimes of touch regions are bounded by the parent DrawableSlide.
    protected override double InitialLifetimeOffset => double.MaxValue;

    public new SlideCheckpoint.CheckpointNode HitObject => (SlideCheckpoint.CheckpointNode)base.HitObject;

    private DrawableSlideCheckpoint checkpoint => (DrawableSlideCheckpoint)ParentHitObject;

    public override bool HandlePositionalInput => true;
    public override bool DisplayResult => false;

    public const float DETECTION_RADIUS = 100;

    public DrawableSlideCheckpointNode()
        : this(null)
    {
    }

    public DrawableSlideCheckpointNode(SlideCheckpoint.CheckpointNode? node)
        : base(node)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.None;
        Size = new Vector2(DETECTION_RADIUS * 2);
        CornerExponent = 2f;
        CornerRadius = DETECTION_RADIUS;
    }

    protected override void OnApply()
    {
        base.OnApply();
        Position = HitObject.Position;
    }

    private int pressedCount;

    protected override void Update()
    {
        base.Update();

        int updatedPressedCounts = countActiveTouchPoints();

        if (updatedPressedCounts > pressedCount)
            UpdateResult(true);

        pressedCount = updatedPressedCounts;
    }

    protected override void CheckForResult(bool userTriggered, double timeOffset)
    {
        // Don't allow for user input if auto is enabled for touch based objects (AutoTouch mod)
        if (!userTriggered || Auto)
        {
            if (timeOffset > 0 && Auto)
                ApplyResult(Result.Judgement.MaxResult);
            return;
        }

        if (!checkpoint.IsHittable)
            return;

        ApplyResult(Result.Judgement.MaxResult);
    }

    [Resolved]
    private SentakkiInputManager sentakkiInputManager { get; set; } = null!;

    private int countActiveTouchPoints()
    {
        var touchInput = sentakkiInputManager.CurrentState.Touch;
        int count = 0;

        if (ReceivePositionalInputAt(sentakkiInputManager.CurrentState.Mouse.Position))
        {
            foreach (var item in sentakkiInputManager.PressedActions)
            {
                if (item < SentakkiAction.Key1)
                    ++count;
            }
        }

        foreach (TouchSource source in touchInput.ActiveSources)
        {
            if (touchInput.GetTouchPosition(source) is Vector2 touchPosition && ReceivePositionalInputAt(touchPosition))
                ++count;
        }

        return count;
    }

    public new void ApplyResult(HitResult result)
    {
        if (Judged)
            return;

        base.ApplyResult(result);
    }
}
