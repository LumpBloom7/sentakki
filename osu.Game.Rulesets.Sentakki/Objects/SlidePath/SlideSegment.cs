namespace osu.Game.Rulesets.Sentakki.Objects.SlidePath;

public record struct SlideSegment
{
    public PathShape Shape { get; set; }

    /// <summary>
    /// The end lane of the slide segment is relative to the segment itself
    /// Essentially, it means that every segment is built with the assumption that they start from lane 0.
    /// </summary>
    public int RelativeEndLane { get; set; }

    public bool Mirrored { get; set; }

    public SlideSegment(PathShape shape, int relativeEndLane, bool mirrored)
    {
        Shape = shape;
        RelativeEndLane = relativeEndLane;
        Mirrored = mirrored;
    }
}
