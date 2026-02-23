using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.Slides;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;

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

    private IReadOnlyList<Drawable> additionalHoverPoints;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        => additionalHoverPoints.Any(d => d.ReceivePositionalInputAt(screenSpacePos)) || visual.ReceivePositionalInputAt(screenSpacePos);

    public SlideSegmentHighlight(Slide slide, int segmentIndex)
    {
        this.slide = slide;
        SegmentIndex = segmentIndex;

        InternalChildren = [
            visual = new SlideVisual()
            {
                SlideBodyInfo = new SlideBodyInfo
                {
                    Segments = [segment]
                }
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Rotation = 22.5f,
                Children = additionalHoverPoints = [
                    new Circle
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 0,
                        Size = new Vector2(60),
                        Y = -SentakkiPlayfield.INTERSECTDISTANCE,
                        AlwaysPresent = true,
                    },
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Rotation = segment.RelativeEndLane * 45f,
                        Child =  new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            Size = new Vector2(60),
                            Y = -SentakkiPlayfield.INTERSECTDISTANCE,
                            AlwaysPresent = true,
                        }
                    }
                ]
            },
        ];
    }

    protected override bool OnHover(HoverEvent e)
    {
        Colour = Color4.Orange;
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        Colour = Color4.White;
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

        yield return new OsuMenuItem("Duplicate segment", MenuItemType.Standard, action: duplicateSegment);

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

    private void changeShape(PathShape shape)
    {
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
        var segments = slideBodyInfo.Segments.ToList();

        segments[SegmentIndex] = segments[SegmentIndex] with { RelativeEndLane = lane };

        slideBodyInfo.Segments = segments;
        beatmap.Update(slide);
    }
}
