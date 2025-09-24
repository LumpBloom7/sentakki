using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private SlideEditorToolboxGroup slidePlacementToolbox { get; set; } = null!;

    [Resolved]
    private SentakkiHitObjectComposer composer { get; set; } = null!;

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
            }
        ]);

        highlight.SlideTapPiece.Y = -SentakkiPlayfield.INTERSECTDISTANCE;
        highlight.SlideTapPiece.Scale = Vector2.One;
    }

    [Resolved]
    private SentakkiSnapProvider snapProvider { get; set; } = null!;

    protected override void Update()
    {
        base.Update();
        highlight.Rotation = HitObject.Lane.GetRotationForLane();
        highlight.SlideTapPiece.Y = -snapProvider.GetDistanceRelativeToCurrentTime(HitObject.StartTime, SentakkiPlayfield.NOTESTARTDISTANCE);
    }

    private SlideBodyInfo? commitedSlideBodyInfo = null!;
    private SlideBodyInfo previewSlideBodyInfo = null!;
    private int currentLaneOffset;

    private readonly Bindable<SlideBodyPart> currentPart = new Bindable<SlideBodyPart>();
    private readonly Bindable<float> shootDelay = new Bindable<float>();

    protected override void LoadComplete()
    {
        base.LoadComplete();

        currentPart.BindTo(slidePlacementToolbox.CurrentPartBindable);
        currentPart.BindValueChanged(v =>
        {
            previewSlideBodyInfo = new SlideBodyInfo
            {
                SlidePathParts = [v.NewValue]
            };
            bodyHighlight.Path = previewSlideBodyInfo.SlidePath;
        }, true);

        shootDelay.BindTo(slidePlacementToolbox.ShootDelayBindable);
        shootDelay.BindValueChanged(v =>
        {
            if (commitedSlideBodyInfo is not null)
                commitedSlideBodyInfo.ShootDelay = v.NewValue;
        });
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (PlacementActive != PlacementState.Active)
        {
            if (e.Button != MouseButton.Left)
                return false;

            BeginPlacement(true);

            EditorClock.SeekSmoothlyTo(HitObject.StartTime);

            HitObject.SlideInfoList.Add(commitedSlideBodyInfo = new SlideBodyInfo()
            {
                ShootDelay = shootDelay.Value
            });

            commited.Rotation = HitObject.Lane.GetRotationForLane();
            bodyHighlight.Rotation = HitObject.Lane.GetRotationForLane();
            bodyHighlight.Alpha = 0.8f;
        }
        else
        {
            if (e.Button == MouseButton.Left)
                commitCurrentPart();
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

            default:
                if (slidePlacementToolbox.HandleKeyDown(e))
                    return true;

                break;
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
            Debug.Assert(commitedSlideBodyInfo is not null);
            double endTime = fallbackTime;

            HitObject.StartTime = endTime < originalStartTime ? endTime : originalStartTime;
            commitedSlideBodyInfo.Duration = Math.Abs(endTime - originalStartTime);

            var localSpacePointerCoord = ToLocalSpace(result.ScreenSpacePosition);

            if ((localSpacePointerCoord - OriginPosition).LengthSquared > 400 * 400)
                return result;

            int newPo = (senRes.Lane - currentLaneOffset - HitObject.Lane).NormalizeLane();

            if (targetPathOffset != newPo)
            {
                slidePlacementToolbox.RequestLaneChange(newPo, true);
                targetPathOffset = newPo;
            }
        }
        else
        {
            HitObject.Lane = senRes.Lane;
            HitObject.StartTime = originalStartTime = result.Time ?? fallbackTime;
        }

        return result;
    }

    private void commitCurrentPart()
    {
        Debug.Assert(commitedSlideBodyInfo is not null);
        laneOffsets.Push(slidePlacementToolbox.CurrentPart.EndOffset);
        bodyParts.Add(slidePlacementToolbox.CurrentPart);

        currentLaneOffset += slidePlacementToolbox.CurrentPart.EndOffset;
        commitedSlideBodyInfo.SlidePathParts = bodyParts.ToArray();
        commited.Path = commitedSlideBodyInfo.SlidePath;

        bodyHighlight.Rotation = (HitObject.Lane + currentLaneOffset).GetRotationForLane();
    }

    private void uncommitLastPart()
    {
        Debug.Assert(commitedSlideBodyInfo is not null);
        if (laneOffsets.Count == 0)
            return;

        currentLaneOffset -= laneOffsets.Pop();
        bodyParts.RemoveAt(bodyParts.Count - 1);

        commitedSlideBodyInfo.SlidePathParts = bodyParts.ToArray();
        commited.Path = commitedSlideBodyInfo.SlidePath;

        bodyHighlight.Rotation = (HitObject.Lane + currentLaneOffset).GetRotationForLane();
    }
}
