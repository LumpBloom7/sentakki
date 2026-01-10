using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Sentakki.Edit.Snapping;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;

public partial class SlidePlacementBlueprint : SentakkiPlacementBlueprint<Slide>
{
    [Resolved]
    private LaneNoteSnapGrid snapGrid { get; set; } = null!;

    private readonly SlideTapPiece tapHighlight;

    private readonly SlideBodyInfo committedSlideInfo;

    private readonly SlideVisual commitedSlideVisual;
    private readonly SlideVisual activeSegmentVisual;

    public SlidePlacementBlueprint()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;

        HitObject.SlideInfoList = [committedSlideInfo = new SlideBodyInfo()];

        AddInternal(new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Colour = Color4.YellowGreen,
            Children =
            [
                commitedSlideVisual = new SlideVisual
                {
                    Rotation = -22.5f,
                    Alpha = 0.5f,
                    SlideBodyInfo = committedSlideInfo,
                },
                activeSegmentVisual = new SlideVisual
                {
                    Alpha = 0,
                    Rotation = -22.5f,
                    SlideBodyInfo = new SlideBodyInfo
                    {
                        Segments = [new SlideSegment(PathShape.Straight, 4, false)]
                    },
                },
                tapHighlight = new SlideTapPiece
                {
                    Alpha = 0.5f,

                    Scale = Vector2.One,
                    SecondStar = { Alpha = 0 }
                }
            ]
        });
    }

    private readonly Bindable<double> animationSpeed = new Bindable<double>(5);

    [BackgroundDependencyLoader]
    private void load(SentakkiBlueprintContainer blueprintContainer)
    {
        animationSpeed.BindTo(blueprintContainer.Composer.DrawableRuleset.AdjustedAnimDuration);
    }

    protected override void Update()
    {
        base.Update();

        InternalChild.Rotation = HitObject.Lane.GetRotationForLane();
        tapHighlight.Y = -Interpolation.ValueAt(
            HitObject.StartTime,
            SentakkiPlayfield.INTERSECTDISTANCE,
            SentakkiPlayfield.NOTESTARTDISTANCE,
            EditorClock.CurrentTime,
            EditorClock.CurrentTime + animationSpeed.Value / 2
        );

        activeSegmentVisual.Rotation = committedSlideInfo.RelativeEndLane.GetRotationForLane() - 45f;
    }

    private double commitStartTime;

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        switch (PlacementActive)
        {
            case PlacementState.Waiting:
                if (e.Button is not MouseButton.Left)
                    break;

                if (!IsValidForPlacement)
                    break;

                BeginPlacement(true);
                activeSegmentVisual.Show();
                commitStartTime = HitObject.StartTime;
                return true;

            case PlacementState.Active:
                switch (e.Button)
                {
                    case MouseButton.Left:
                        if (e.AltPressed)
                        {
                            currentSegment.Mirrored = !currentSegment.Mirrored;
                            activeSegmentVisual.SlideBodyInfo!.Segments = [currentSegment];
                            return true;
                        }

                        committedSlideInfo.Segments = [.. committedSlideInfo.Segments, currentSegment];
                        return true;

                    case MouseButton.Middle:

                        if (committedSlideInfo.Segments.Count == 0)
                            break;

                        committedSlideInfo.Segments = [.. committedSlideInfo.Segments.SkipLast(1)];
                        return true;

                    case MouseButton.Right:
                        EndPlacement(HitObject.SlideInfoList[0].Segments.Count > 0);
                        return true;
                }

                break;
        }

        return base.OnMouseDown(e);
    }

    private SlideSegment currentSegment = new SlideSegment(PathShape.Straight, 4, false);

    public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double time)
    {
        Vector2 localMousePosition = ToLocalSpace(screenSpacePosition) - OriginPosition;
        (double snappedTime, int lane) = snapGrid.GetSnappedTimeAndPosition(time, localMousePosition);

        switch (PlacementActive)
        {
            case PlacementState.Waiting:
                time = snappedTime;
                HitObject.Lane = lane;
                break;

            case PlacementState.Active:
                // If the mapper maps the hold in reverse direction, we swap the start and end times to ensure correctness.
                HitObject.StartTime = Math.Min(commitStartTime, time);
                double endTime = Math.Max(commitStartTime, time);

                HitObject.SlideInfoList[0].Duration = endTime - HitObject.StartTime;

                int startLane = committedSlideInfo.RelativeEndLane + HitObject.Lane;

                var newSegment = currentSegment with { RelativeEndLane = lane - startLane };

                if (!SlidePaths.CheckSlideValidity(newSegment))
                    break;

                currentSegment = newSegment;
                activeSegmentVisual.SlideBodyInfo!.Segments = [currentSegment];
                break;
        }

        return base.UpdateTimeAndPosition(screenSpacePosition, time);
    }

    protected override bool OnScroll(ScrollEvent e)
    {
        if (PlacementActive is not PlacementState.Active || !e.AltPressed)
            return base.OnScroll(e);

        var newSegment = currentSegment with { Shape = (PathShape)(((int)currentSegment.Shape + (int)e.ScrollDelta.Y) % 7) };

        if (newSegment.Shape < 0)
            newSegment.Shape += 7;

        while (!SlidePaths.CheckSlideValidity(newSegment))
        {
            newSegment.RelativeEndLane = (newSegment.RelativeEndLane + (currentSegment.RelativeEndLane > 4 ? 7 : 1)) % 8;
        }

        currentSegment = newSegment;
        activeSegmentVisual.SlideBodyInfo!.Segments = [currentSegment];

        return true;
    }

    public override void EndPlacement(bool commit)
    {
        base.EndPlacement(commit);

        // Return the chevrons to the pool when placement ends
        commitedSlideVisual.SlideBodyInfo = null;
        activeSegmentVisual.SlideBodyInfo = null;
    }
}
