using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides;

public partial class SlideSegmentHighlight : CompositeDrawable, IHasContextMenu
{
    [Resolved]
    private EditorBeatmap beatmap { get; set; } = null!;

    private Slide slide;
    public int SegmentIndex { get; private set; }

    private SlideBodyInfo slideBodyInfo => slide.SlideInfoList[0];
    private SlideSegment segment => slideBodyInfo.Segments[SegmentIndex];

    private SlideVisual visual;
    private Drawable dragDotContainer;
    private DotPiece dragDot;

    private IBindable<int> versionBindable;

    [Resolved]
    private SlideTapPiece slideTapPiece { get; set; } = null!;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
    {
        if (IsDragged)
            return true;

        // Special behaviour: We don't want to allow the slide segment to take control when the target is actually the tap.
        if (slideTapPiece.ReceivePositionalInputAt(screenSpacePos))
            return false;

        return visual.ReceivePositionalInputAt(screenSpacePos) || dragDot.ReceivePositionalInputAt(screenSpacePos);
    }

    public SlideSegmentHighlight(Slide slide, int segmentIndex)
    {
        this.slide = slide;
        SegmentIndex = segmentIndex;

        InternalChildren = [
            visual = new SlideVisual(),
            dragDotContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = [
                    dragDot = new DotPiece
                    {
                        Y = -SentakkiPlayfield.INTERSECTDISTANCE,
                    },
                ]
            },
        ];

        versionBindable = slideBodyInfo.Version.GetBoundCopy();
        versionBindable.BindValueChanged(_ => updateSelf(), true);
    }

    private void updateSelf()
    {
        int offset = slideBodyInfo.Segments.Take(SegmentIndex).Sum(s => s.RelativeEndLane);
        Rotation = offset * 45;
        visual.SlideBodyInfo = new SlideBodyInfo
        {
            Segments = [segment]
        };

        dragDotContainer.Rotation = (segment.RelativeEndLane * 45) + 22.5f;
    }

    [Resolved]
    private SentakkiBlueprintContainer blueprintContainer { get; set; } = null!;

    private IEnumerable<MenuItem> createContextMenuItems()
    {
        foreach (var item in blueprintContainer.SelectionHandler.GetContextMenuItemsForSelection())
            yield return item;

        yield return new OsuMenuItemSpacer();

        List<TernaryStateRadioMenuItem> shapeMenuItems = [];

        foreach (var shape in Enum.GetValues<PathShape>())
        {
            shapeMenuItems.Add(new TernaryStateRadioMenuItem($"{shape}", action: _ => changeShape(shape))
            {
                State = { Value = segment.Shape == shape ? TernaryState.True : TernaryState.False }
            });
        }

        yield return new OsuMenuItem("Shape")
        {
            Items = shapeMenuItems,
        };

        List<(int, TernaryStateRadioMenuItem)> endLaneItems = [];

        int currentEndLane = (slide.Lane + slideBodyInfo.Segments.Take(SegmentIndex + 1).Sum(s => s.RelativeEndLane)).NormalizeLane();
        int startLane = (currentEndLane - segment.RelativeEndLane).NormalizeLane();

        foreach (int i in Enumerable.Range(0, 8))
        {
            if (!SlidePaths.CheckSlideValidity(segment with { RelativeEndLane = i }))
                continue;

            int displayIndex = (i + startLane).NormalizeLane() + 1;
            endLaneItems.Add((displayIndex, new TernaryStateRadioMenuItem($"{displayIndex}", action: _ => changeEndLane(i))
            {
                State = { Value = segment.RelativeEndLane == i ? TernaryState.True : TernaryState.False }
            }));
        }

        yield return new OsuMenuItem("End Lane")
        {
            Items = [.. endLaneItems.OrderBy(i => i.Item1).Select(i => i.Item2)]
        };

        if (SlidePaths.CheckSlideValidity(segment with { Mirrored = !segment.Mirrored }, true))
        {
            yield return new TernaryStateToggleMenuItem("Mirrored", action: _ => mirrorSegment())
            {
                State = { Value = segment.Mirrored ? TernaryState.True : TernaryState.False }
            };
        }

        yield return new OsuMenuItem("Duplicate segment", MenuItemType.Standard, action: duplicateSegment);

        if (slideBodyInfo.Segments.Count > 1)
            yield return new OsuMenuItem("Delete segment", MenuItemType.Destructive, deleteSegment);

        yield return new OsuMenuItemSpacer();

        yield return new OsuMenuItem("Delete slide", MenuItemType.Destructive, deleteSlide);
    }

    public MenuItem[]? ContextMenuItems => [.. createContextMenuItems()];

    private void deleteSlide()
    {
        beatmap.Remove(slide);
    }

    private void deleteSegment()
    {
        slideBodyInfo.Segments = [.. slideBodyInfo.Segments.Where((v, i) => i != SegmentIndex)];
        beatmap.Update(slide);
    }

    private void duplicateSegment()
    {
        List<SlideSegment> updatedSegments = [.. slideBodyInfo.Segments];

        updatedSegments.Insert(SegmentIndex, segment);

        slideBodyInfo.Segments = updatedSegments;
        beatmap.Update(slide);
    }

    private void mirrorSegment()
    {
        var segments = slideBodyInfo.Segments.ToList();

        var segment = segments[SegmentIndex];
        segment.Mirrored = !segment.Mirrored;

        segments[SegmentIndex] = segment;

        slideBodyInfo.Segments = segments;
        beatmap.Update(slide);
    }

    private void changeShape(PathShape shape)
    {
        if (segment.Shape == shape)
            return;

        var segments = slideBodyInfo.Segments.ToList();

        var candidateSegment = segment with { Shape = shape };
        int relativeEnd = segment.RelativeEndLane;

        int offset = 1;
        while (!SlidePaths.CheckSlideValidity(candidateSegment))
        {
            candidateSegment.RelativeEndLane = (relativeEnd + offset).NormalizeLane();
            if (SlidePaths.CheckSlideValidity(candidateSegment))
                break;

            candidateSegment.RelativeEndLane = (relativeEnd - offset).NormalizeLane();
            if (SlidePaths.CheckSlideValidity(candidateSegment))
                break;

            ++offset;
        }

        segments[SegmentIndex] = candidateSegment;

        slideBodyInfo.Segments = segments;
        beatmap.Update(slide);
    }

    private void changeEndLane(int lane)
    {
        if (segment.RelativeEndLane == lane)
            return;

        var candidateSegment = segment with { RelativeEndLane = lane };

        if (!SlidePaths.CheckSlideValidity(candidateSegment))
            return;

        var segments = slideBodyInfo.Segments.ToList();

        segments[SegmentIndex] = candidateSegment;

        slideBodyInfo.Segments = segments;
        beatmap.Update(slide);
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button != MouseButton.Middle)
            return base.OnMouseDown(e);

        if (slideBodyInfo.Segments.Count == 1)
            deleteSlide();
        else
            deleteSegment();

        return true;
    }

    protected override void OnDrag(DragEvent e) => handleDragEvent(e.ScreenSpaceMousePosition);

    private bool dragOccured;

    protected override bool OnDragStart(DragStartEvent e)
    {
        dragOccured = true;
        return true;
    }

    protected override void OnMouseUp(MouseUpEvent e)
    {
        // HACK: Blueprint container will attempt to perform selection actions, jankily supress it.
        if (dragOccured)
            blueprintContainer.SuppressMouseUp();

        base.OnMouseUp(e);
    }

    protected override bool OnHover(HoverEvent e)
    {
        Colour = Color4.Orange;
        dragDot.ScaleTo(1.3f, 50);

        // We don't "handle" the hover, allowing the blueprint container to accept click events
        return false;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        Colour = Color4.White;
        dragDot.ScaleTo(1.0f, 100);
    }

    private void handleDragEvent(Vector2 screenSpacePosition)
    {
        if (!IsPresent)
            return;

        var localMousePosition = ToLocalSpace(screenSpacePosition);

        float angle = OriginPosition.AngleTo(localMousePosition);

        int lane = (int)Math.Round((angle - 22.5f) / 45);

        int currentStartLane = slideBodyInfo.Segments.Take(SegmentIndex).Sum(s => s.RelativeEndLane);

        changeEndLane(lane.NormalizeLane());
    }
}
