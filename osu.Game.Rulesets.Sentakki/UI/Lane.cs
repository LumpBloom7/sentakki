using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
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

    private const float receptor_angle_range_inner = receptor_angle_range * 1.4f;
    private const float receptor_angle_range_inner_mid = receptor_angle_range_inner / 2;

    private SentakkiInputManager? sentakkiActionInputManager;
    internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= (SentakkiInputManager)GetContainingInputManager();

    public override bool HandlePositionalInput => true;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
    {
        var localPos = ToLocalSpace(screenSpacePos);

        float distance = Vector2.DistanceSquared(Vector2.Zero, localPos);
        if (distance is < 200 * 200 or > 600 * 600) return false;

        float targetAngleRangeMid = distance > 400 ? receptor_angle_range_mid : receptor_angle_range_inner_mid;

        float angleDelta = MathExtensions.AngleDelta(0, Vector2.Zero.AngleTo(localPos));

        return !(Math.Abs(angleDelta) > targetAngleRangeMid);
    }

    private void updateInputState()
    {
        updateTouchInputState();
        updateMouseInputState();
    }

    private readonly Dictionary<TouchSource, bool> touchInputState = [];

    private void updateTouchInputState()
    {
        var touchInput = SentakkiActionInputManager.CurrentState.Touch;

        for (TouchSource t = TouchSource.Touch1; t <= TouchSource.Touch10; ++t)
        {
            bool wasDetected = touchInputState.GetValueOrDefault(t);
            bool isDetected = touchInput.GetTouchPosition(t) is Vector2 touchPosition && ReceivePositionalInputAt(touchPosition);

            touchInputState[t] = isDetected;

            switch (isDetected)
            {
                case false when wasDetected:
                    SentakkiActionInputManager.TriggerReleased(SentakkiAction.SensorLane1 + LaneNumber);
                    break;

                case true when !wasDetected:
                    SentakkiActionInputManager.TriggerPressed(SentakkiAction.SensorLane1 + LaneNumber);
                    break;
            }
        }
    }

    private readonly Dictionary<SentakkiAction, bool> buttonInputState = [];

    private void updateMouseInputState()
    {
        for (SentakkiAction a = SentakkiAction.Button1; a <= SentakkiAction.Button2; ++a)
        {
            bool wasDetected = buttonInputState.GetValueOrDefault(a);
            bool isDetected = IsHovered && SentakkiActionInputManager.PressedActions.Contains(a);

            buttonInputState[a] = isDetected;

            SentakkiAction action = (a is SentakkiAction.Button1 ? SentakkiAction.B1Lane1 : SentakkiAction.B2Lane1) + LaneNumber;

            switch (isDetected)
            {
                case false when wasDetected:
                    SentakkiActionInputManager.TriggerReleased(action);
                    break;

                case true when !wasDetected:
                    SentakkiActionInputManager.TriggerPressed(action);
                    break;
            }
        }
    }

    #endregion
}
