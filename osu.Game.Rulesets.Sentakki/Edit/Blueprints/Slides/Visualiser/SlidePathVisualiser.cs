using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Sentakki.Extensions;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides.Visualiser;

public partial class SlidePathVisualiser : CompositeDrawable, IHasContextMenu
{
    private readonly Container<SmoothPath> paths;
    private readonly Container<SmoothPath> hoverPaths;

    private readonly SlideBodyInfo slideBodyInfo;
    private readonly Slide slide;

    [Resolved(CanBeNull = true)]
    private EditorBeatmap? editorBeatmap { get; set; }

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => hoverPaths.Children.Any(c => c.ReceivePositionalInputAt(screenSpacePos));

    public SlidePathVisualiser(Slide slide, SlideBodyInfo slideBodyInfo, int startLane)
    {
        this.slide = slide;
        this.slideBodyInfo = slideBodyInfo;
        slideBodyInfo.OnPathUpdated += reloadVisualiser;
        RelativeSizeAxes = Axes.Both;
        Alpha = 0;

        InternalChildren =
        [
            paths = new Container<SmoothPath>
            {
                RelativeSizeAxes = Axes.Both,
            },
            hoverPaths = new Container<SmoothPath>
            {
                RelativeSizeAxes = Axes.Both
            },
            new SlideOffsetVisualiser(slide, slideBodyInfo)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.TopCentre,
                Rotation = 22.5f,
            }
        ];

        loadPaths();

        breakStateBindable = new Bindable<TernaryState>()
        {
            Value = slideBodyInfo.Break ? TernaryState.True : TernaryState.False,
        };

        exStateBindable = new Bindable<TernaryState>()
        {
            Value = slideBodyInfo.Ex ? TernaryState.True : TernaryState.False,
        };

        breakStateBindable.BindValueChanged(setBreakState);
        exStateBindable.BindValueChanged(setExState);

        updateContextMenuItems();

        // HACK: This seems to be necessary in order for the hover paths to be validated correctly for ReceivePositionAt
        AlwaysPresent = true;
    }

    private void setBreakState(ValueChangedEvent<TernaryState> value)
    {
        editorBeatmap?.BeginChange();
        slideBodyInfo.Break = value.NewValue == TernaryState.True;
        editorBeatmap?.Update(slide);
        editorBeatmap?.EndChange();
    }

    private void setExState(ValueChangedEvent<TernaryState> value)
    {
        editorBeatmap?.BeginChange();
        slideBodyInfo.Ex = value.NewValue == TernaryState.True;
        editorBeatmap?.Update(slide);
        editorBeatmap?.EndChange();
    }

    private void removeSlideBody()
    {
        editorBeatmap?.BeginChange();
        slide.SlideInfoList.Remove(slideBodyInfo);
        editorBeatmap?.Update(slide);
        editorBeatmap?.EndChange();
    }

    private void updateContextMenuItems()
    {
        List<MenuItem> cmItems =
        [
            new TernaryStateToggleMenuItem("Break", MenuItemType.Standard)
            {
                State = { BindTarget = breakStateBindable }
            },
            new TernaryStateToggleMenuItem("Ex", MenuItemType.Standard)
            {
                State = { BindTarget = exStateBindable }
            },
        ];

        cmItems.Add(new OsuMenuItem("Delete", MenuItemType.Destructive, slide.SlideInfoList.Count > 1 ? removeSlideBody : null));

        ContextMenuItems = [.. cmItems];
    }

    private readonly Bindable<TernaryState> breakStateBindable;
    private readonly Bindable<TernaryState> exStateBindable;

    public MenuItem[]? ContextMenuItems { get; private set; }

    private void loadPaths()
    {
        paths.Clear();
        hoverPaths.Clear();

        int currentOffset = 0;

        float hueInterval = 1f / slideBodyInfo.SlidePathParts.Length;

        float hue = 0;

        List<Vector2> fullVertices = [];

        for (int i = 0; i < slideBodyInfo.SlidePathParts.Length; ++i)
        {
            var part = slideBodyInfo.SlidePathParts[i];

            if (part.Shape is SlidePaths.PathShapes.Fan)
                part = new SlideBodyPart(SlidePaths.PathShapes.Straight, 4, false);

            var sentakkiSlidePath = SlidePaths.CreateSlidePath(part);

            var segmentColour = Colour4.FromHSL(hue, 1, 0.5f);

            List<Vector2> vertices = [];

            foreach (var path in sentakkiSlidePath.SlideSegments)
            {
                List<Vector2> partVertices = [];

                path.GetPathToProgress(partVertices, 0, 1);
                vertices.AddRange(partVertices);
            }

            fullVertices.AddRange(vertices);

            SmoothPath smoothPath = new EditorSlidePartPiece(slide, slideBodyInfo, i)
            {
                PathRadius = 5,
                AccentColour = segmentColour,
                Vertices = vertices,
                Anchor = Anchor.Centre,
                Position = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, currentOffset),
                Rotation = currentOffset.GetRotationForLane() - 22.5f
            };

            SmoothPath hoverPath = new SmoothPath()
            {
                PathRadius = 25,
                Alpha = 0f,
                Vertices = vertices,
                Anchor = Anchor.Centre,
                Position = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, currentOffset),
                Rotation = currentOffset.GetRotationForLane() - 22.5f,
            };

            hoverPath.OriginPosition = hoverPath.PositionInBoundingBox(hoverPath.Vertices[0]);
            smoothPath.OriginPosition = smoothPath.PositionInBoundingBox(smoothPath.Vertices[0]);

            currentOffset += part.EndOffset;
            hue += hueInterval;

            paths.Add(smoothPath);
            hoverPaths.Add(hoverPath);
        }
    }

    private void reloadVisualiser()
    {
        loadPaths();
    }

    public void Select()
    {
        Alpha = 1;
        paths.Alpha = 1;
        updateContextMenuItems();
    }

    public void Deselect()
    {
        Alpha = 0;
        paths.Alpha = 0;
        ContextMenuItems = null;
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);

        slideBodyInfo.OnPathUpdated -= reloadVisualiser;
    }
}
