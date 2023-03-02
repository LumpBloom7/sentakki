using System;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SlideBodyPart : IEquatable<SlideBodyPart>
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

        public override bool Equals(object? obj) => obj is not null && obj is SlideBodyPart otherPart && Equals(otherPart);

        public bool Equals(SlideBodyPart? other) => other is not null && (ReferenceEquals(this, other) || (Shape == other.Shape && Mirrored == other.Mirrored && EndOffset == other.EndOffset));

        public override int GetHashCode() => HashCode.Combine(Shape, EndOffset, Mirrored);
    }
}
