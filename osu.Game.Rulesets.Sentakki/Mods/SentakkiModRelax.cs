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
    private bool tapPressed;
    private bool holdPressed;

    private bool needsTap;
    private bool needsHold;

    private Vector2? previousMousePosition;
    private Vector2 predictedMousePosition;
    private int hoveredLane;
    private int predictedLane;

    public void ApplyToDrawableRuleset(DrawableRuleset<SentakkiHitObject> drawableRuleset)
    {
        var drawableSentakkiRuleset = (DrawableSentakkiRuleset)drawableRuleset;

        inputManager = drawableSentakkiRuleset.SentakkiInputManager;
        lanes = ((SentakkiPlayfield)drawableSentakkiRuleset.Playfield).LanedPlayfield.Lanes;
    }

    public void ApplyToPlayer(Player player)
    {
        if (inputManager.ReplayInputHandler != null)
        {
            hasReplay = true;
            return;
        }

        inputManager.AllowUserPresses = false;
    }

    public void Update(Playfield playfield)
    {
        if (hasReplay)
            return;

        var mousePosition = inputManager.CurrentState.Mouse.Position;

        predictedMousePosition = mousePosition + (mousePosition - (previousMousePosition ?? mousePosition));
        previousMousePosition = mousePosition;

        hoveredLane = getHoveredLane(mousePosition);
        predictedLane = getHoveredLane(predictedMousePosition);
        needsTap = false;
        needsHold = false;

        checkPlayfield(playfield, playfield.Clock.CurrentTime, mousePosition);

        updateTapButton();
        updateHoldButton();
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

    private void checkPlayfield(Playfield playfield, double time, Vector2 mousePosition)
    {
        foreach (var drawableHitObject in playfield.HitObjectContainer.AliveObjects)
            checkHitObject(drawableHitObject, time, mousePosition);

        foreach (var nestedPlayfield in playfield.NestedPlayfields)
            checkPlayfield(nestedPlayfield, time, mousePosition);
    }

    private void checkHitObject(DrawableHitObject drawableHitObject, double time, Vector2 mousePosition)
    {
        if (drawableHitObject.AllJudged)
            return;

        switch (drawableHitObject)
        {
            case DrawableHold hold when time <= hold.HitObject.EndTime && hold.HitObject.Lane == hoveredLane:
                if (!isInHitRange(hold.Head.HitObject, time, hold.HitObject.Lane != predictedLane))
                    break;

                needsHold = true;
                needsTap |= !hold.Head.AllJudged;
                break;

            case DrawableTap tap when tap.HitObject.Lane == hoveredLane:
                needsTap |= isInHitRange(tap.HitObject, time, tap.HitObject.Lane != predictedLane) && canBeHit(tap, time);
                break;

            case DrawableTouchHold touchHold when time >= touchHold.HitObject.StartTime && time <= touchHold.HitObject.EndTime && touchHold.ReceivePositionalInputAt(mousePosition):
                needsHold = true;
                break;

            case DrawableTouch touch when touch.ReceivePositionalInputAt(mousePosition):
                needsTap |= isInHitRange(drawableHitObject.HitObject, time, !touch.ReceivePositionalInputAt(predictedMousePosition)) && canBeHit(touch, time);
                break;

            case DrawableSlide slide:
                checkSlide(slide, time, mousePosition);
                break;
        }
    }

    private void checkSlide(DrawableSlide slide, double time, Vector2 mousePosition)
    {
        foreach (var slideTap in slide.SlideTaps)
            checkHitObject(slideTap, time, mousePosition);

        foreach (var body in slide.SlideBodies)
        {
            foreach (var checkpoint in body.SlideCheckpoints)
            {
                if (checkpoint.AllJudged || !checkpoint.IsHittable)
                    continue;

                foreach (var node in checkpoint.NestedHitObjects)
                {
                    if (!node.AllJudged && node.ReceivePositionalInputAt(mousePosition))
                        needsTap = true;
                }
            }
        }
    }

    private static bool isInHitRange(HitObject hitObject, double time, bool isLeaving)
    {
        double offset = time - hitObject.StartTime;

        if (hitObject.HitWindows is not { } hitWindows)
            return offset >= 0;

        double criticalWindow = hitWindows.WindowFor(HitResult.Perfect);
        double perfectWindow = hitWindows.WindowFor(HitResult.Great);

        return offset >= -criticalWindow || (isLeaving && offset >= -perfectWindow);
    }

    private static bool canBeHit(DrawableHitObject drawableHitObject, double time)
    {
        var hitObject = drawableHitObject.HitObject;

        return hitObject.HitWindows?.CanBeHit(time - hitObject.StartTime) ?? true;
    }

    private void updateTapButton()
    {
        bool shouldPress = needsTap && !tapPressed;

        if (shouldPress == tapPressed)
            return;

        tapPressed = shouldPress;
        setButton(SentakkiAction.Button1, tapPressed);
    }

    private void updateHoldButton()
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
}
