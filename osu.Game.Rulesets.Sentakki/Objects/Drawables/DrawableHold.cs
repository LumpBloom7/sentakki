using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables;

public partial class DrawableHold : DrawableSentakkiLanedHitObject, IKeyBindingHandler<SentakkiAction>
{
    public new Hold HitObject => (Hold)base.HitObject;
    public DrawableHoldHead Head => headContainer.Child;

    private Container<DrawableHoldHead> headContainer = null!;

    public HoldBody NoteBody = null!;

    public override double LifetimeStart
    {
        get => base.LifetimeStart;
        set
        {
            base.LifetimeStart = value;
            NoteBody.LifetimeStart = value;
        }
    }

    public override double LifetimeEnd
    {
        get => base.LifetimeEnd;
        set
        {
            base.LifetimeEnd = value;
            NoteBody.LifetimeEnd = value;
        }
    }

    public DrawableHold()
        : this(null)
    {
    }

    public DrawableHold(Hold? hitObject = null)
        : base(hitObject)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        AddRangeInternal(
        [
            NoteBody = new HoldBody
            {
                Scale = Vector2.Zero,
                Y = -SentakkiPlayfield.NOTESTARTDISTANCE
            },
            headContainer = new Container<DrawableHoldHead> { RelativeSizeAxes = Axes.Both },
        ]);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        AccentColour.BindValueChanged(c => flashingColour = AccentColour.Value.LightenHsl(0.4f), true);
    }

    private Color4 flashingColour = Color4.White;

    protected override void OnApply()
    {
        base.OnApply();
        releaseTime = null;
        holdAttempted = false;
    }

    private double? releaseTime;
    private bool isHolding => releaseTime is null;

    private bool holdAttempted;

    protected override void Update()
    {
        base.Update();

        autoplayUpdate();

        if (AllJudged)
        {
            // Remove alterations to NoteBody colour
            NoteBody.Colour = AccentColour.Value;
            return;
        }

        // Ensure that the note colour is correct prior to the start time
        if (Time.Current < HitObject.StartTime)
        {
            Colour = Color4.White;
            NoteBody.Colour = AccentColour.Value;
            return;
        }

        if (!isHolding)
        {
            // Remove alterations to NoteBody colour
            NoteBody.Colour = AccentColour.Value;

            // Grey the note to indicate that it isn't being held
            Colour = Interpolation.ValueAt(
                Math.Clamp(Time.Current, HitObject.StartTime, HitObject.StartTime + 100),
                Color4.White, Color4.SlateGray,
                HitObject.StartTime, HitObject.StartTime + 100, Easing.OutSine);

            return;
        }

        // Restore colour if it is being held
        Colour = Color4.White;

        const double flashing_time = 80;

        double flashProg = Time.Current % (flashing_time * 2) / (flashing_time * 2);

        if (flashProg <= 0.5)
            NoteBody.Colour = Interpolation.ValueAt(flashProg, AccentColour.Value, flashingColour, 0, 0.5, Easing.OutSine);
        else
            NoteBody.Colour = Interpolation.ValueAt(flashProg, flashingColour, AccentColour.Value, 0.5, 0, Easing.InSine);
    }


    protected override void UpdateInitialTransforms()
    {
        base.UpdateInitialTransforms();
        double animTime = AnimationDuration.Value / 2;
        NoteBody.FadeInFromZero(animTime).ScaleTo(1, animTime);

        using (BeginDelayedSequence(animTime))
        {
            // This is the movable length (not including start position)
            const float total_movable_distance = SentakkiPlayfield.INTERSECTDISTANCE - SentakkiPlayfield.NOTESTARTDISTANCE;

            // This is the amount of stretch needed. Capped to the max stretch amount.
            float stretchAmount = Math.Clamp((float)(total_movable_distance / animTime * (HitObject as IHasDuration).Duration), 0, total_movable_distance);

            // This is the amount of time that the note spends stretching or unstretching
            float stretchTime = (float)(stretchAmount / total_movable_distance * animTime);

            NoteBody.MoveToY(-SentakkiPlayfield.INTERSECTDISTANCE, animTime) // Move the head towards the ring
                    .ResizeHeightTo(stretchAmount, stretchTime) // While we are moving, we stretch the hold note to match desired length
                    .Then().Delay(HitObject.Duration - stretchTime) // Wait until the end of the hold note, while considering how much time we need for shrinking
                    .ResizeHeightTo(0, stretchTime); // We shrink the hold note as it exits
        }
    }

    protected override void CheckForResult(bool userTriggered, double timeOffset)
    {
        if (userTriggered)
            return;

        // Judgement of the tail can only happen after the head is judged.
        if (!Head.Judged)
            return;

        double perfectWindow = HitObject.HitWindows.WindowFor(HitResult.Perfect);
        double timeNotHeld = releaseTime.HasValue ? Time.Current - releaseTime.Value : 0;

        // If the player is still holding it beyond the perfect window, the maximum result is a Great.
        if (timeOffset > perfectWindow && isHolding)
        {
            ApplyResult(HitResult.Great);
        }
        else if (timeOffset >= -perfectWindow && !isHolding)
        {
            // If the player never attempted a hold, we just consider it a miss.
            if (!holdAttempted)
            {
                ApplyResult(HitObject.Judgement.MinResult);
                return;
            }

            // If the user is not holding the note, also take into account the time the player wasn't holding the note
            var earlyReleaseResult = HitObject.HitWindows.ResultFor(timeOffset - timeNotHeld);

            if (earlyReleaseResult <= HitResult.None)
                earlyReleaseResult = HitResult.Miss;

            ApplyResult(earlyReleaseResult);
        }
        // If the user hasn't held it for 200ms, unconditionally consider a miss.
        else if (timeNotHeld >= HitObject.HitWindows.WindowFor(HitResult.Miss))
        {
            ApplyResult(HitObject.Judgement.MinResult);
        }
    }

    protected override void UpdateHitStateTransforms(ArmedState state)
    {
        base.UpdateHitStateTransforms(state);
        const double time_fade_miss = 400;

        switch (state)
        {
            case ArmedState.Hit:
                NoteBody.FadeOut();
                this.FadeOut();
                break;

            case ArmedState.Miss:
                NoteBody.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                        .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                        .MoveToOffset(new Vector2(0, -100), time_fade_miss, Easing.OutCubic)
                        .FadeOut(time_fade_miss);

                this.Delay(time_fade_miss).FadeOut();
                break;
        }

        Expire();
    }

    protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
    {
        switch (hitObject)
        {
            case Hold.HoldHead head:
                return new DrawableHoldHead(head)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AutoBindable = { BindTarget = AutoBindable }
                };
        }

        return base.CreateNestedHitObject(hitObject);
    }

    protected override void AddNestedHitObject(DrawableHitObject hitObject)
    {
        base.AddNestedHitObject(hitObject);

        switch (hitObject)
        {
            case DrawableHoldHead head:
                headContainer.Child = head;
                break;
        }
    }

    protected override void ClearNestedHitObjects()
    {
        base.ClearNestedHitObjects();
        headContainer.Clear(false);
    }

    [Resolved]
    private SentakkiInputManager sentakkiInputManager { get; set; } = null!;

    private int pressedCount
    {
        get
        {
            int count = 0;

            foreach (var pressedAction in sentakkiInputManager.PressedActions)
            {
                if (IsValidLaneAction(pressedAction))
                    ++count;
            }

            return count;
        }
    }

    public bool OnPressed(KeyBindingPressEvent<SentakkiAction> e)
    {
        if (AllJudged)
            return false;

        if (!IsValidLaneAction(e.Action))
            return false;

        // Passthrough excess inputs to later hitobjects in the same lane
        if (isHolding)
            return false;

        double timeOffset = Time.Current - HitObject.StartTime;

        if (timeOffset < -Head.HitObject.HitWindows.WindowFor(HitResult.Miss))
            return false;

        Head.UpdateResult();

        holdAttempted = true;
        releaseTime = null;
        return true;
    }

    public void OnReleased(KeyBindingReleaseEvent<SentakkiAction> e)
    {
        if (AllJudged) return;
        if (!isHolding) return;

        if (!IsValidLaneAction(e.Action))
            return;

        // We only release the hold once ALL inputs are released
        // We check for 1 here as drawables receive the event before the counter decrements
        if (pressedCount > 1)
            return;

        releaseTime = Time.Current;
    }

    private void autoplayUpdate()
    {
        if (!Auto)
            return;

        // If auto is within the hittable time, attempt to hit it
        // HACK: In editor context, frame stability is not enforced, this could potentially lead to 0 duration slides being missed as we never ever visit the window.
        // We resolve this by giving autoplay a bit more leniency. In practice nothing should change for regular autoplay.
        double missWindow = HitObject.HitWindows.WindowFor(HitResult.Miss);

        if (Time.Current >= HitObject.StartTime && Time.Current < HitObject.EndTime + missWindow)
        {
            if (!isHolding)
                Head.UpdateResult();

            holdAttempted = true;
        }

        // Pretend that a release was made if auto is holding the note beyond end time
        if (Time.Current >= HitObject.GetEndTime())
            releaseTime = Time.Current;
    }
}
