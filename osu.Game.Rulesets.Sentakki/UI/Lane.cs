using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.UI;

public partial class Lane : Playfield
{
    public int LaneNumber { get; init; }

    public Action<Drawable>? OnLoaded = null!;

    public Lane()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.None;
        AddInternal(HitObjectContainer);
    }

    protected override void Update()
    {
        base.Update();
        updateInputState();
    }

    [Resolved]
    private DrawableSentakkiRuleset drawableSentakkiRuleset { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RegisterPool<Tap, DrawableTap>(8);

        RegisterPool<Hold, DrawableHold>(8);
        RegisterPool<Hold.HoldHead, DrawableHoldHead>(8);

        RegisterPool<Slide, DrawableSlide>(2);
        RegisterPool<SlideTap, DrawableSlideTap>(2);
        RegisterPool<SlideBody, DrawableSlideBody>(2);
        RegisterPool<SlideCheckpoint, DrawableSlideCheckpoint>(18);
        RegisterPool<SlideCheckpoint.CheckpointNode, DrawableSlideCheckpointNode>(18);

        RegisterPool<ScorePaddingObject, DrawableScorePaddingObject>(20);
    }

    protected override void OnNewDrawableHitObject(DrawableHitObject d) => OnLoaded?.Invoke(d);

    protected override HitObjectLifetimeEntry CreateLifetimeEntry(HitObject hitObject) => new SentakkiHitObjectLifetimeEntry(hitObject, drawableSentakkiRuleset);

    #region Input Handling

    private const float receptor_angle_range = 45;
    private const float receptor_angle_range_mid = receptor_angle_range / 2;

    [Resolved]
    private SentakkiInputManager sentakkiInputManager { get; set; } = null!;

    public override bool HandlePositionalInput => true;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
    {
        var localPos = ToLocalSpace(screenSpacePos);

        float distance = Vector2.DistanceSquared(Vector2.Zero, localPos);
        if (distance is < 200 * 200 or > 600 * 600) return false;

        float angleDelta = MathExtensions.AngleDelta(0, Vector2.Zero.AngleTo(localPos));

        return !(Math.Abs(angleDelta) > receptor_angle_range_mid);
    }

    private TouchSource? blockingTouchSource;
    private double blockingTouchTime;

    // This method handles direct touches to the centre, and not touches carried from a slide
    // This has logic to block subsequent touches made in close succession.
    protected override bool OnTouchDown(TouchDownEvent e)
    {
        const double blocking_time = 10;

        // Mark the touch point as seen, unconditionally.
        touchInputState[e.Touch.Source] = true;

        if (blockingTouchSource is not null && Time.Current - blockingTouchTime < blocking_time)
            return false;

        blockingTouchTime = Time.Current;
        blockingTouchSource = e.Touch.Source;
        triggerTouchDown();

        return true;
    }

    private void updateInputState()
    {
        updateTouchInputState();
        updateMouseInputState();
    }

    private readonly Dictionary<TouchSource, bool> touchInputState = [];

    private uint touchCount;

    private void updateTouchInputState()
    {
        var touchInput = sentakkiInputManager.CurrentState.Touch;

        for (TouchSource t = TouchSource.Touch1; t <= TouchSource.Touch10; ++t)
        {
            bool wasDetected = touchInputState.GetValueOrDefault(t);
            bool isDetected = touchInput.GetTouchPosition(t) is Vector2 touchPosition && ReceivePositionalInputAt(touchPosition);

            touchInputState[t] = isDetected;

            switch (isDetected)
            {
                case false when wasDetected:
                    // If this was the point that triggered a direct touch block, unblock.
                    if (blockingTouchSource == t)
                        blockingTouchSource = null;

                    triggerTouchRelease();

                    break;

                case true when !wasDetected:
                    triggerTouchDown();
                    break;
            }
        }
    }

    private void triggerTouchDown()
    {
        if (touchCount >= 2)
            return;

        SentakkiAction baseAction = SentakkiAction.SensorLane1 + (int)(touchCount * 8);
        ++touchCount;

        sentakkiInputManager.TriggerPressed(baseAction + LaneNumber);
    }

    private void triggerTouchRelease()
    {
        switch (touchCount)
        {
            case <= 0:
                return;

            case 1:
            {
                int remainingFingers = 0;

                foreach (var t in touchInputState)
                {
                    if (t.Value)
                        ++remainingFingers;
                }

                if (remainingFingers >= 1)
                    return;

                break;
            }
        }

        --touchCount;
        SentakkiAction baseAction = SentakkiAction.SensorLane1 + (int)(touchCount * 8);

        sentakkiInputManager.TriggerReleased(baseAction + LaneNumber);
    }

    private readonly Dictionary<SentakkiAction, bool> buttonInputState = [];

    private void updateMouseInputState()
    {
        for (SentakkiAction a = SentakkiAction.Button1; a <= SentakkiAction.Button2; ++a)
        {
            bool wasDetected = buttonInputState.GetValueOrDefault(a);
            bool isDetected = IsHovered && sentakkiInputManager.PressedActions.Contains(a);

            buttonInputState[a] = isDetected;

            SentakkiAction action = (a is SentakkiAction.Button1 ? SentakkiAction.B1Lane1 : SentakkiAction.B2Lane1) + LaneNumber;

            switch (isDetected)
            {
                case false when wasDetected:
                    sentakkiInputManager.TriggerReleased(action);
                    break;

                case true when !wasDetected:
                    sentakkiInputManager.TriggerPressed(action);
                    break;
            }
        }
    }

    #endregion
}
