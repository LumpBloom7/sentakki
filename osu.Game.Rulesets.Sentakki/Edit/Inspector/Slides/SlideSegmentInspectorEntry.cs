using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Inspector.Slides;

public partial class SentakkiSlideSegmentInspectorEntry : CompositeDrawable, IHasPopover
{
    private OsuSpriteText text;

    private SlideBodyInfo slideBodyInfo;
    private int segmentIndex;

    private SlideSegment segment => slideBodyInfo.Segments[segmentIndex];
    private Slide slide;

    public SentakkiSlideSegmentInspectorEntry(Slide slide, SlideBodyInfo slideBodyInfo, int index)
    {
        this.slide = slide;
        this.slideBodyInfo = slideBodyInfo;
        segmentIndex = index;

        AutoSizeAxes = Axes.Both;

        InternalChild = text = new OsuSpriteText();
        text.Font = text.Font.With(weight: FontWeight.SemiBold);
    }

    private void updateText()
    {
        int simpleEndOffset = segment.RelativeEndLane;
        if (simpleEndOffset > 4)
            simpleEndOffset -= 8;

        string mirrored = "";

        switch (segment.Shape)
        {
            case PathShape.Circle:
                mirrored = segment.Mirrored ? "CCW" : "CW";
                break;

            case PathShape.U:
            case PathShape.Cup:
            case PathShape.Thunder:
                mirrored = segment.Mirrored ? "M" : "";
                break;
        }

        text.Text = $"{segment.Shape}({simpleEndOffset}){mirrored}";
    }

    private Bindable<Visibility> popoverVisibilityState = new();
    private IBindable<int> versionBindable = new Bindable<int>();

    protected override void LoadComplete()
    {
        base.LoadComplete();

        versionBindable.BindTo(slideBodyInfo.Version);
        versionBindable.BindValueChanged(_ => updateText(), true);
        popoverVisibilityState.BindValueChanged(v => Colour = v.NewValue == Visibility.Visible ? Color4.YellowGreen : Color4.White);
    }


    public Popover? GetPopover() => new SegmentEditPopover(slide, slideBodyInfo, segmentIndex)
    {
        State = { BindTarget = popoverVisibilityState }
    };

    protected override bool OnClick(ClickEvent e)
    {
        this.ShowPopover();
        return true;
    }

    private partial class SegmentEditPopover : OsuPopover
    {
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        private Slide slide;
        private SlideBodyInfo slideBodyInfo;
        private int segmentIndex;

        private SlideSegment segment => slideBodyInfo.Segments[segmentIndex];

        private IBindable<int> versionBindable;

        public SegmentEditPopover(Slide slide, SlideBodyInfo slideBodyInfo, int index)
        {
            this.slide = slide;
            this.slideBodyInfo = slideBodyInfo;
            segmentIndex = index;

            versionBindable = slideBodyInfo.Version.GetBoundCopy();
        }

        private FormEnumDropdown<PathShape> shapeDropdown = null!;
        private FormCheckBox mirroredCheckbox = null!;
        private FormDropdown<int> endLaneDropdown = null!;

        private RoundedButton deleteButton = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                Width = 240,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(5),
                Children = [
                    new OsuSpriteText
                    {
                        Text = "Edit segment",
                        Font = OsuFont.Style.Heading2
                    },
                    shapeDropdown = new FormEnumDropdown<PathShape>()
                    {
                        Caption = "Shape",
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,

                        RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                        ColumnDimensions = [new Dimension(GridSizeMode.Relative, 0.4f), new Dimension(GridSizeMode.Absolute, 5), new Dimension(GridSizeMode.Distributed)],
                        Content = new Drawable?[][]
                        {
                            [
                                endLaneDropdown = new FormDropdown<int>()
                                {
                                    Caption = "End Lane",
                                },
                                null,
                                mirroredCheckbox = new FormCheckBox()
                                {
                                    Caption = "Mirrored",
                                },
                            ]
                        }
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,

                        RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                        ColumnDimensions = [new Dimension(GridSizeMode.Distributed), new Dimension(GridSizeMode.Absolute, 5), new Dimension(GridSizeMode.Distributed)],

                        Content = new Drawable?[][]
                        {
                            [
                                new RoundedButton()
                                {
                                    Text = "Duplicate",
                                    RelativeSizeAxes = Axes.X,
                                    Enabled = { Value = true },
                                    Action = duplicateSegment
                                },
                                null,
                                deleteButton = new DangerousRoundedButton()
                                {
                                    Text = "Delete",
                                    RelativeSizeAxes = Axes.X,
                                    Enabled = { Value = true },
                                    Action = deleteSegment
                                }
                            ]
                        }
                    }
                ]
            };

            versionBindable.BindValueChanged(v => refreshForms(), true);
            shapeDropdown.Current.BindValueChanged(_ => updateShape());
            mirroredCheckbox.Current.BindValueChanged(_ => updateShape());
            endLaneDropdown.Current.BindValueChanged(_ => updateShape());
        }

        private void refreshForms()
        {
            if (slideBodyInfo.Segments.Count <= segmentIndex)
                return;

            shapeDropdown.Current.Value = segment.Shape;
            mirroredCheckbox.Current.Value = segment.Mirrored;

            int currentEndLane = (slide.Lane + slideBodyInfo.Segments.Take(segmentIndex + 1).Sum(s => s.RelativeEndLane)).NormalizeLane();
            int startLane = (currentEndLane - segment.RelativeEndLane).NormalizeLane();

            // We have to temporarily re-enable the bindable to be able to change it.
            endLaneDropdown.Current.Disabled = false;

            endLaneDropdown.Items = Enumerable.Range(0, 8).Where(i => SlidePaths.CheckSlideValidity(segment with { RelativeEndLane = i })).Select(i => (i + startLane).NormalizeLane() + 1).Order();
            endLaneDropdown.Current.Value = currentEndLane + 1;
            endLaneDropdown.Current.Disabled = endLaneDropdown.Items.Count() == 1;

            deleteButton.Enabled.Value = slideBodyInfo.Segments.Count > 1;
        }

        private void updateShape()
        {
            List<SlideSegment> updatedSegments = [.. slideBodyInfo.Segments];

            int currentEndLane = (slide.Lane + slideBodyInfo.Segments.Take(segmentIndex + 1).Sum(s => s.RelativeEndLane)).NormalizeLane();
            int startLane = (currentEndLane - segment.RelativeEndLane).NormalizeLane();

            int relativeEnd = (endLaneDropdown.Current.Value - 1 - startLane).NormalizeLane();

            var candidateSegment = new SlideSegment
            {
                Shape = shapeDropdown.Current.Value,
                Mirrored = mirroredCheckbox.Current.Value,
                RelativeEndLane = relativeEnd
            };

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

            updatedSegments[segmentIndex] = candidateSegment;

            slideBodyInfo.Segments = updatedSegments;
            editorBeatmap.Update(slide);
        }

        private void deleteSegment()
        {
            slideBodyInfo.Segments = [.. slideBodyInfo.Segments.Where((v, i) => i != segmentIndex)];
            editorBeatmap.Update(slide);
        }

        private void duplicateSegment()
        {
            List<SlideSegment> updatedSegments = [.. slideBodyInfo.Segments];
            updatedSegments.Insert(segmentIndex, segment);

            slideBodyInfo.Segments = updatedSegments;
            editorBeatmap.Update(slide);
        }
    }
}
