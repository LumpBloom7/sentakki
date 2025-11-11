namespace osu.Game.Rulesets.Sentakki.Objects.SlidePath;

public record struct SlideSegment
{
    public PathShapes Shape { get; set; }
    public int EndOffset { get; set; }
    public bool Mirrored { get; set; }

    public SlideSegment(PathShapes shape, int endOffset, bool mirrored)
    {
        Shape = shape;
        EndOffset = endOffset;
        Mirrored = mirrored;
    }
}
