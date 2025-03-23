using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Edit.Blueprints.Slides.Visualiser;

[Cached]
public partial class SlidePathVisualiser : CompositeDrawable
{
    private Container<SmoothPath> paths;

    private readonly SlideBodyInfo slideBodyInfo;
    private readonly int startLane;
    private readonly Slide slide;

    public SlidePathVisualiser(Slide slide, SlideBodyInfo slideBodyInfo, int startLane)
    {
        this.slide = slide;
        this.slideBodyInfo = slideBodyInfo;
        this.startLane = startLane;
        RelativeSizeAxes = Axes.Both;

        InternalChildren = [
            paths = new Container<SmoothPath>{
                RelativeSizeAxes = Axes.Both,
            }
        ];

        loadPaths();
    }

    private void loadPaths()
    {
        paths.Clear();

        int currentOffset = startLane;
        Console.WriteLine(currentOffset);

        float hueInterval = 1f / slideBodyInfo.SlidePathParts.Length;

        float hue = 0;

        for (int i = 0; i < slideBodyInfo.SlidePathParts.Length; ++i)
        {
            var part = slideBodyInfo.SlidePathParts[i];

            var sentakkiSlidePath = SlidePaths.CreateSlidePath(part);

            var segmentColour = Colour4.FromHSL(hue, 1, 0.5f);

            List<Vector2> vertices = [];

            foreach (var path in sentakkiSlidePath.SlideSegments)
            {
                List<Vector2> partVertices = [];

                path.GetPathToProgress(partVertices, 0, 1);
                vertices.AddRange(partVertices);
            }

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
    }

    public void ReloadVisualiser()
    {
        loadPaths();
    }
}
