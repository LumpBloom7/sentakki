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

        public override bool Equals(object obj)
        {
            if (obj is not SlideBodyPart otherPart)
                return false;

            return Equals(otherPart);
        }

        public bool Equals(SlideBodyPart other) => Shape == other.Shape && EndOffset == EndOffset;

        public override int GetHashCode() => HashCode.Combine(Shape, EndOffset, Mirrored);
    }
}
