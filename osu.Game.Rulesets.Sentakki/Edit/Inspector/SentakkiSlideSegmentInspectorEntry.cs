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
using osu.Game.Rulesets.Sentakki.Objects.SlidePath;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Edit.Inspector;

public partial class SentakkiSlideSegmentInspectorEntry : CompositeDrawable, IHasPopover
{
    private OsuSpriteText text;

    private SlideBodyInfo slideBodyInfo;
    private SlideSegment segment;

    public SentakkiSlideSegmentInspectorEntry(SlideBodyInfo slideBodyInfo, SlideSegment segment)
    {
        AutoSizeAxes = Axes.Both;
        this.slideBodyInfo = slideBodyInfo;
        this.segment = segment;

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

        InternalChildren = [
            text = new OsuSpriteText{
                Text = $"{segment.Shape}({simpleEndOffset}){mirrored}",
            },
        ];

        text.Font = text.Font.With(weight: FontWeight.SemiBold);
    }

    private Bindable<Visibility> popoverVisibilityState = new();

    protected override void LoadComplete()
    {
        base.LoadComplete();

        popoverVisibilityState.BindValueChanged(v => Colour = v.NewValue == Visibility.Visible ? Color4.YellowGreen : Color4.White);
    }


    public Popover? GetPopover() => new SegmentEditPopover(slideBodyInfo, segment)
    {
        State = { BindTarget = popoverVisibilityState }
    };

    protected override bool OnClick(ClickEvent e)
    {
        popoverVisibilityState.Value = Visibility.Visible;
        this.ShowPopover();
        return true;
    }

    private partial class SegmentEditPopover : OsuPopover
    {
        private SlideBodyInfo slideBodyInfo;
        private SlideSegment segment;

        public SegmentEditPopover(SlideBodyInfo slideBodyInfo, in SlideSegment segment)
        {
            this.slideBodyInfo = slideBodyInfo;
            this.segment = segment;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                Width = 220,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(5),
                Children = [
                    new OsuSpriteText{
                        Text = "Edit segment",
                        Font = OsuFont.Style.Heading2
                    },
                    new FormEnumDropdown<PathShape>(){
                        Caption = "Segment shape",
                        Current = {Value = segment.Shape}
                    },
                    new FormCheckBox(){
                        Caption = "Mirror shape",
                        Current = {Value = segment.Mirrored}
                    },
                    new FormDropdown<int>(){
                        Caption = "End offset",
                        Items = Enumerable.Range(-3,8).Where(i => SlidePaths.CheckSlideValidity(segment with {RelativeEndLane = i.NormalizeLane()})),

                        Current = {
                            Value= Enumerable.Range(-3,8).First(i => i.NormalizeLane() == segment.RelativeEndLane)
                        }
                    },
                    new GridContainer{
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,

                        RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                        ColumnDimensions = [new Dimension(GridSizeMode.Distributed), new Dimension(GridSizeMode.Absolute, 5), new Dimension(GridSizeMode.Distributed)],

                        Content = new Drawable?[][]{
                            [
                                new RoundedButton(){
                                    Text = "Duplicate",
                                    RelativeSizeAxes = Axes.X,
                                    Enabled = {Value = true}
                                },
                                null,
                                new DangerousRoundedButton(){
                                    Text = "Delete",
                                    RelativeSizeAxes = Axes.X,
                                    Enabled = {Value = true}
                                }
                            ]
                        }
                    }

                ]
            };
        }
    }

}