using System;
using System.Collections.Generic;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Localisation.Mods;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Mods;

public class SentakkiModRelax : ModRelax, IUpdatableByPlayfield, IApplicableToDrawableRuleset<SentakkiHitObject>, IApplicableToPlayer, IHasNoTimedInputs
{
    public override LocalisableString Description => SentakkiModRelaxStrings.ModDescription;
    public override Type[] IncompatibleMods => [.. base.IncompatibleMods, typeof(SentakkiModTouchDevice)];

    private SentakkiInputManager inputManager = null!;
    private List<Lane> lanes = null!;

    private bool hasReplay;

    public void ApplyToDrawableRuleset(DrawableRuleset<SentakkiHitObject> drawableRuleset)
    {
        var drawableSentakkiRuleset = (DrawableSentakkiRuleset)drawableRuleset;

        inputManager = drawableSentakkiRuleset.SentakkiInputManager;
        lanes = ((SentakkiPlayfield)drawableSentakkiRuleset.Playfield).LanedPlayfield.Lanes;
    }

    public void ApplyToPlayer(Player player)
    {
        if (inputManager.ReplayInputHandler is not null)
        {
            hasReplay = true;
            return;
        }

        inputManager.AllowUserPresses = false;
    }

    private Vector2? previousMousePosition;

    public void Update(Playfield playfield)
    {
        if (hasReplay)
            return;

        var mousePosition = inputManager.CurrentState.Mouse.Position;
        Vector2 predictedMousePosition = mousePosition + (mousePosition - (previousMousePosition ?? mousePosition));
        previousMousePosition = mousePosition;

        var mousePositionLookahead = new LookaheadValue<Vector2>(mousePosition, predictedMousePosition);
        var lane = new LookaheadValue<int>(getHoveredLane(mousePosition), getHoveredLane(predictedMousePosition));

        (bool needsTap, bool needsHold) = checkPlayfield(playfield, playfield.Clock.CurrentTime, mousePositionLookahead, lane);

        updateTapButton(needsTap);
        updateHoldButton(needsHold);
    }

    private int getHoveredLane(Vector2 mousePosition)
    {
        for (int i = 0; i < lanes.Count; ++i)
        {
            if (lanes[i].ReceivePositionalInputAt(mousePosition))
                return i;
        }

        return -1;
    }

    private (bool needsTap, bool needsHold) checkPlayfield(Playfield playfield, double time, LookaheadValue<Vector2> mousePosition, LookaheadValue<int> hoveredLane)
    {
        bool needsTap = false;
        bool needsHold = false;

        foreach (var drawableHitObject in playfield.HitObjectContainer.AliveObjects)
        {
            var result = checkHitObject(drawableHitObject, time, mousePosition, hoveredLane);
            needsTap |= result.needsTap;
            needsHold |= result.needsHold;

            // Both are needed, no point looking at other objects, since they can never be false again
            if (needsTap && needsHold)
                return (true, true);
        }

        foreach (var nestedPlayfield in playfield.NestedPlayfields)
        {
            var result = checkPlayfield(nestedPlayfield, time, mousePosition, hoveredLane);
            needsTap |= result.needsTap;
            needsHold |= result.needsHold;

            // Both are needed, no point looking at other objects, since they can never be false again
            if (needsTap && needsHold)
                break;
        }

        return (needsTap, needsHold);
    }

    private (bool needsTap, bool needsHold) checkHitObject(DrawableHitObject drawableHitObject, double time, LookaheadValue<Vector2> mousePosition, LookaheadValue<int> hoveredLane)
    {
        if (drawableHitObject.AllJudged)
            return (false, false);

        switch (drawableHitObject)
        {
            case DrawableHold hold:
                if (time > hold.HitObject.EndTime || hold.HitObject.Lane != hoveredLane.Current)
                    break;

                bool isHeadHittable = isInHitRange(hold.Head.HitObject, time, hold.HitObject.Lane != hoveredLane.Predicted);

                // If we can't properly hit the head yet, ignore everything
                if (!isHeadHittable)
                    break;

                return (needsTap: !hold.Head.AllJudged, needsHold: true);

            case DrawableTap tap:
                if (tap.HitObject.Lane != hoveredLane.Current)
                    break;

                return (
                    needsTap: canBeHit(tap, time) && isInHitRange(tap.HitObject, time, tap.HitObject.Lane != hoveredLane.Predicted),
                    needsHold: false
                );

            case DrawableTouchHold touchHold:
                if (time <= touchHold.HitObject.StartTime || time > touchHold.HitObject.EndTime)
                    break;

                if (!touchHold.ReceivePositionalInputAt(mousePosition.Current))
                    break;

                return (needsTap: false, needsHold: true);

            case DrawableTouch touch:
                if (!touch.ReceivePositionalInputAt(mousePosition.Current))
                    break;

                return (
                    needsTap: canBeHit(touch, time) && isInHitRange(drawableHitObject.HitObject, time, !touch.ReceivePositionalInputAt(mousePosition.Predicted)),
                    needsHold: false);

            case DrawableSlide slide:
                foreach (var slideTap in slide.SlideTaps)
                {
                    if (checkHitObject(slideTap, time, mousePosition, hoveredLane).needsTap)
                        return (needsTap: true, needsHold: false);
                }

                foreach (var slideBody in slide.SlideBodies)
                {
                    if (checkSlideBody(slideBody, mousePosition))
                        return (needsTap: true, needsHold: false);
                }

                break;
        }

        return (false, false);
    }

    private static bool checkSlideBody(DrawableSlideBody slidebody, LookaheadValue<Vector2> mousePosition)
    {
        foreach (var checkpoint in slidebody.SlideCheckpoints)
        {
            if (checkpoint.AllJudged || !checkpoint.IsHittable)
                continue;

            foreach (var node in checkpoint.NestedHitObjects)
            {
                if (!node.AllJudged && node.ReceivePositionalInputAt(mousePosition.Current))
                    return true;
            }
        }

        return false;
    }

    private static bool isInHitRange(HitObject hitObject, double time, bool isLeaving)
    {
        double offset = time - hitObject.StartTime;

        if (hitObject.HitWindows is not HitWindows hitWindows)
            return offset >= 0;

        double perfectWindow = hitWindows.WindowFor(HitResult.Great);

        return offset >= 0 || (isLeaving && offset >= -perfectWindow);
    }

    private static bool canBeHit(DrawableHitObject drawableHitObject, double time)
    {
        var hitObject = drawableHitObject.HitObject;

        return hitObject.HitWindows?.CanBeHit(time - hitObject.StartTime) ?? true;
    }

    private bool tapPressed;

    private void updateTapButton(bool needsTap)
    {
        bool shouldPress = needsTap && !tapPressed;


        if (shouldPress == tapPressed)
            return;

        tapPressed = shouldPress;
        setButton(SentakkiAction.Button1, tapPressed);
    }

    private bool holdPressed;

    private void updateHoldButton(bool needsHold)
    {
        if (needsHold == holdPressed)
            return;

        holdPressed = needsHold;
        setButton(SentakkiAction.Button2, holdPressed);
    }

    private void setButton(SentakkiAction action, bool pressed)
    {
        if (pressed)
            inputManager.TriggerPressed(action);
        else
            inputManager.TriggerReleased(action);
    }

    private record struct LookaheadValue<T>(T Current, T Predicted);
}
