namespace osu.Game.Rulesets.Sentakki.Objects.SlidePath;

public record struct SlideSegment
{
    public PathShapes Shape { get; set; }

    /// <summary>
    /// The end lane of the slide segment is relative to the segment itself
    /// Essentially, it means that every segment is built with the assumption that they start from lane 0.
    /// </summary>
    public int RelativeEndLane { get; set; }

    public bool Mirrored { get; set; }

    public SlideSegment(PathShapes shape, int relativeEndLane, bool mirrored)
    {
        Shape = shape;
        RelativeEndLane = relativeEndLane;
        Mirrored = mirrored;
    }
}
