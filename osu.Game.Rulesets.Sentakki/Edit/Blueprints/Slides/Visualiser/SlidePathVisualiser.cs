using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides.Visualiser;

[Cached]
public partial class SlidePathVisualiser : CompositeDrawable
{
    private Container<SmoothPath> paths;
    private Container<SmoothPath> hoverPaths;

    private readonly SlideBodyInfo slideBodyInfo;
    private readonly Slide slide;

    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => hoverPaths.Child.ReceivePositionalInputAt(screenSpacePos);

    public SlidePathVisualiser(Slide slide, SlideBodyInfo slideBodyInfo, int startLane)
    {
        this.slide = slide;
        this.slideBodyInfo = slideBodyInfo;
        RelativeSizeAxes = Axes.Both;
        Alpha = 0;

        InternalChildren = [
            paths = new Container<SmoothPath>
            {
                RelativeSizeAxes = Axes.Both,
            },
            hoverPaths = new Container<SmoothPath>
            {
                RelativeSizeAxes = Axes.Both
            },
            new SlideOffsetVisualiser(slide, slideBodyInfo){
                Anchor = Anchor.Centre,
                Origin = Anchor.TopCentre,
                Rotation = 22.5f,
            }
        ];

        loadPaths();
    }

    private void loadPaths()
    {
        paths.Clear();

        int currentOffset = 0;
        Console.WriteLine(currentOffset);

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

            smoothPath.OriginPosition = smoothPath.PositionInBoundingBox(smoothPath.Vertices[0]);
            currentOffset += part.EndOffset;
            hue += hueInterval;

            paths.Add(smoothPath);
        }

        var hoverPath = hoverPaths.Child = new SmoothPath()
        {
            PathRadius = 25,
            Alpha = 0f,
            Vertices = fullVertices,
            Anchor = Anchor.Centre,
            Position = SentakkiExtensions.GetPositionAlongLane(SentakkiPlayfield.INTERSECTDISTANCE, 0),
            Rotation = 0
        };
        hoverPath.OriginPosition = hoverPath.PositionInBoundingBox(hoverPath.Vertices[0]);
    }

    public void ReloadVisualiser()
    {
        loadPaths();
    }

    public void Select()
    {
        Alpha = 1;
    }

    public void Deselect()
    {
        Alpha = 0;
    }
}
