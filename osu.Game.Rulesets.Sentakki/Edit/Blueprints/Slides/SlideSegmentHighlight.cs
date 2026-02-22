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
    private int segmentIndex;

    private IBindable<int> versionBindable;

    private SlideBodyInfo slideBodyInfo => slide.SlideInfoList[0];
    private SlideSegment segment => slideBodyInfo.Segments[segmentIndex];



    private SlideVisual visual;

    private IReadOnlyList<Drawable> additionalHoverPoints;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        => additionalHoverPoints.Any(d => d.ReceivePositionalInputAt(screenSpacePos)) || visual.ReceivePositionalInputAt(screenSpacePos);

    public SlideSegmentHighlight(Slide slide, int segmentIndex)
    {
        this.slide = slide;
        this.segmentIndex = segmentIndex;

        versionBindable = slideBodyInfo.Version.GetBoundCopy();

        InternalChildren = [
            visual = new SlideVisual(){
                SlideBodyInfo = new SlideBodyInfo{
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

        yield return new OsuMenuItem("Shape")
        {
            Items = [.. Enum.GetValues<PathShape>().Select(s => new TernaryStateRadioMenuItem($"{s}"))],
        };

        yield return new OsuMenuItem("End Lane")
        {
            Items = [.. Enumerable.Range(1, 8).Select(s => new TernaryStateRadioMenuItem($"{s}"))]
        };

        yield return new OsuMenuItem("Duplicate segment");

        yield return new OsuMenuItem("Delete segment", MenuItemType.Destructive);

        yield return new OsuMenuItemSpacer();

        yield return new OsuMenuItem("Delete slide", MenuItemType.Destructive, deleteSlide);
    }

    public MenuItem[]? ContextMenuItems => [.. createContextMenuItems()];

    private void deleteSlide()
    {
        beatmap.Remove(slide);
    }
}
