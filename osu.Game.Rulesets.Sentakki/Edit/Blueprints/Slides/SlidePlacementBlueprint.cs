using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;

public partial class SlidePlacementBlueprint : SentakkiPlacementBlueprint<Slide>
{
    private readonly SlideTapHighlight highlight;

    private readonly SlideVisual bodyHighlight;
    private readonly SlideVisual commited;
    protected override bool IsValidForPlacement => HitObject.Duration > 0 && commitedSlideBodyInfo?.SlidePathParts?.Length > 0;

    [Resolved]
    private EditorBeatmap beatmap { get; set; } = null!;

    [Resolved]
    private SentakkiHitObjectComposer composer { get; set; } = null!;

    [Resolved]
    private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

    // Placement state
    private SlideBodyPart currentPart = new SlideBodyPart(SlidePaths.PathShapes.Straight, 4, false);
    private int currentLaneOffset;

    private readonly Bindable<double> preferredShootDelay = new Bindable<double>();
    private readonly Bindable<bool> manualShootDelay = new Bindable<bool>();

    private readonly SlideOffsetTool offsetTool;
    private readonly Container offsetToolContainer;

    public SlidePlacementBlueprint()
    {
        AddRangeInternal([
            highlight = new SlideTapHighlight(),
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Rotation = -22.5f,
                Children =
                [
                    commited = new SlideVisual
                    {
                        Colour = Color4.LimeGreen,
                        Alpha = 0.5f,
                    },
                    bodyHighlight = new SlideVisual
                    {
                        Colour = Color4.GreenYellow,
                        Alpha = 0f,
                    }
                ]
            },
            offsetToolContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = offsetTool = new SlideOffsetTool(HitObject)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.TopCentre,
                        ShootDelayBindable = { BindTarget = preferredShootDelay },
                        ManualAdjustmentsMade = { BindTarget = manualShootDelay },
                        State =
                        {
                            Value = Visibility.Visible
                        }
                    }
                }
            }
        ]);

        highlight.SlideTapPiece.Y = -SentakkiPlayfield.INTERSECTDISTANCE;
        highlight.SlideTapPiece.Scale = Vector2.One;
    }

    [Resolved]
    private SentakkiSnapProvider snapProvider { get; set; } = null!;

    protected override void LoadComplete()
    {
        base.LoadComplete();

        updatePreview();
    }

    protected override void Update()
    {
        base.Update();
        offsetToolContainer.Rotation = highlight.Rotation = HitObject.Lane.GetRotationForLane();
        highlight.SlideTapPiece.Y = -snapProvider.GetDistanceRelativeToCurrentTime(HitObject.StartTime, SentakkiPlayfield.NOTESTARTDISTANCE);
        tryUpdateShootDelay();
    }

    private readonly SlideBodyInfo commitedSlideBodyInfo = new SlideBodyInfo();
    private readonly SlideBodyInfo previewSlideBodyInfo = new SlideBodyInfo();

    private void updatePreview()
    {
        previewSlideBodyInfo.SlidePathParts = [currentPart];
        bodyHighlight.Path = previewSlideBodyInfo.SlidePath;
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (PlacementActive != PlacementState.Active)
        {
            if (e.Button != MouseButton.Left)
                return false;

            BeginPlacement(true);

            EditorClock.SeekSmoothlyTo(HitObject.StartTime);

            HitObject.SlideInfoList.Add(commitedSlideBodyInfo);

            commited.Rotation = HitObject.Lane.GetRotationForLane();
            bodyHighlight.Rotation = HitObject.Lane.GetRotationForLane();
            bodyHighlight.Alpha = 0.8f;
        }
        else
        {
            switch (e.Button)
            {
                case MouseButton.Left:
                    commitCurrentPart();
                    break;

                case MouseButton.Middle:
                    uncommitLastPart();
                    break;
            }
        }

        return true;
    }

    protected override void OnMouseUp(MouseUpEvent e)
    {
        if (e.Button != MouseButton.Right)
            return;

        if (PlacementActive == PlacementState.Active)
        {
            Debug.Assert(commitedSlideBodyInfo is not null);
            EndPlacement(bodyParts.Count > 0 && commitedSlideBodyInfo.Duration > 0);
        }
    }

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        if (PlacementActive != PlacementState.Active)
            return base.OnKeyDown(e);

        switch (e.Key)
        {
            case Key.BackSpace:
                uncommitLastPart();
                break;

            // Dedicated keybinds for shapes
            case Key.Number1:
            case Key.Number2:
            case Key.Number3:
            case Key.Number4:
            case Key.Number5:
            case Key.Number6:
            case Key.Number7:
                if (!e.AltPressed)
                    break;

                int index = e.Key - Key.Number1;

                SlidePaths.PathShapes targetShape = (SlidePaths.PathShapes)index;

                if (currentPart.Shape == targetShape)
                    currentPart.Mirrored = !currentPart.Mirrored;

                currentPart.Shape = targetShape;
                performLaneChange(currentLaneOffset);
                updatePreview();

                return true;

            case Key.D:
                preferredShootDelay.Value += beatSnapProvider.GetBeatLengthAtTime(HitObject.StartTime);
                return true;

            case Key.A:
                preferredShootDelay.Value -= beatSnapProvider.GetBeatLengthAtTime(HitObject.StartTime);
                return true;

            case Key.S:
                preferredShootDelay.Value = beatSnapProvider.GetBeatLengthAtTime(HitObject.StartTime) * beatSnapProvider.BeatDivisor;
                return true;

            case Key.Tab:
                if (e.ControlPressed)
                    currentPart.Mirrored = !currentPart.Mirrored;
                else if (e.ShiftPressed)
                    currentPart.Shape = (SlidePaths.PathShapes)((int)(currentPart.Shape + 6) % 7);
                else
                    currentPart.Shape = (SlidePaths.PathShapes)((int)(currentPart.Shape + 1) % 7);

                performLaneChange(currentLaneOffset);
                updatePreview();
                return true;
        }

        return base.OnKeyDown(e);
    }

    private readonly List<SlideBodyPart> bodyParts = [];

    // Only used to revert the lane offsets after uncommit
    private readonly Stack<int> laneOffsets = new Stack<int>();

    // The path offset of the lane the player is pointing at
    private int targetPathOffset;

    private double originalStartTime;

    public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double fallbackTime)
    {
        var result = composer?.FindSnappedPositionAndTime(screenSpacePosition) ?? new SnapResult(screenSpacePosition, fallbackTime);

        base.UpdateTimeAndPosition(result.ScreenSpacePosition, result.Time ?? fallbackTime);

        if (result is not SentakkiLanedSnapResult senRes)
            return result;

        if (PlacementActive == PlacementState.Active)
        {
            double endTime = fallbackTime;

            HitObject.StartTime = endTime < originalStartTime ? endTime : originalStartTime;
            commitedSlideBodyInfo.Duration = Math.Abs(endTime - originalStartTime);

            var localSpacePointerCoord = ToLocalSpace(result.ScreenSpacePosition);

            if ((localSpacePointerCoord - OriginPosition).LengthSquared > 400 * 400)
                return result;

            int newPo = (senRes.Lane - currentLaneOffset - HitObject.Lane).NormalizeLane();

            if (targetPathOffset != newPo)
            {
                performLaneChange(newPo, true);
                targetPathOffset = newPo;
            }
        }
        else
        {
            HitObject.Lane = senRes.Lane;
            HitObject.StartTime = originalStartTime = result.Time ?? fallbackTime;
            updateCurrentSegmentParameters();
        }

        return result;
    }

    private void commitCurrentPart()
    {
        laneOffsets.Push(currentPart.EndOffset);
        bodyParts.Add(currentPart);

        currentLaneOffset += currentPart.EndOffset;
        commitedSlideBodyInfo.SlidePathParts = bodyParts.ToArray();
        commited.Path = commitedSlideBodyInfo.SlidePath;

        bodyHighlight.Rotation = (HitObject.Lane + currentLaneOffset).GetRotationForLane();
    }

    private void uncommitLastPart()
    {
        if (laneOffsets.Count == 0)
            return;

        currentLaneOffset -= laneOffsets.Pop();
        bodyParts.RemoveAt(bodyParts.Count - 1);

        commitedSlideBodyInfo.SlidePathParts = bodyParts.ToArray();
        commited.Path = commitedSlideBodyInfo.SlidePath;

        bodyHighlight.Rotation = (HitObject.Lane + currentLaneOffset).GetRotationForLane();
    }

    private void performLaneChange(int newLane, bool findClosestMatch = false)
    {
        int oldOffset = currentPart.EndOffset;

        int rotationFactor = newLane - oldOffset >= 0 ? 1 : -1;

        for (int i = 0; i < 8; ++i)
        {
            var newPart = currentPart with { EndOffset = (newLane + (i * rotationFactor)).NormalizeLane() };

            if (SlidePaths.CheckSlideValidity(newPart))
            {
                currentPart.EndOffset = newPart.EndOffset;
                updatePreview();
                return;
            }

            if (findClosestMatch)
                rotationFactor *= -1;
        }
    }

    private void updateCurrentSegmentParameters()
    {
        if (!manualShootDelay.Value)
            preferredShootDelay.Value = beatSnapProvider.GetBeatLengthAtTime(HitObject.StartTime) * beatSnapProvider.BeatDivisor;

        var lastSlidePart = beatmap.HitObjects.OfType<Slide>().TakeWhile(h => h.StartTime <= HitObject.StartTime).LastOrDefault()?.SlideInfoList?.LastOrDefault()?.SlidePathParts.LastOrDefault();

        if (lastSlidePart is null)
        {
            currentPart = new SlideBodyPart(SlidePaths.PathShapes.Straight, 4, false);
            return;
        }

        currentPart = lastSlidePart.Value;
    }

    private void tryUpdateShootDelay()
    {
        commitedSlideBodyInfo.ShootDelay = Math.Clamp(preferredShootDelay.Value, 0, Math.Max(HitObject.Duration - 50, 0));
    }
}
