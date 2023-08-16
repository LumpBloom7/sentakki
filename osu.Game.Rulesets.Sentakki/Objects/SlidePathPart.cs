namespace osu.Game.Rulesets.Sentakki.Objects
{
    public record struct SlideBodyPart
    {
        public SlidePaths.PathShapes Shape { get; private set; }
        public int EndOffset { get; set; }
        public bool Mirrored { get; set; }

        public SlideBodyPart(SlidePaths.PathShapes shape, int endOffset, bool mirrored)
        {
            Shape = shape;
            EndOffset = endOffset;
            Mirrored = mirrored;
        }
    }
}
