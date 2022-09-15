using System;

namespace osu.Game.Rulesets.Sentakki.Objects.SlidePath
{
    public class PathParameters : IEquatable<PathParameters>
    {
        public SlidePaths.PathShapes Shape { get; private set; }
        public int EndOffset { get; set; }
        public bool Mirrored { get; set; }

        public PathParameters(SlidePaths.PathShapes shape, int endOffset, bool mirrored)
        {
            Shape = shape;
            EndOffset = endOffset;
            Mirrored = mirrored;
        }

        public override bool Equals(object obj)
        {
            if (obj is not PathParameters otherPath)
                return false;

            return Equals(otherPath);
        }

        public bool Equals(PathParameters other) => Shape == other.Shape && EndOffset == EndOffset;

        public override int GetHashCode() => HashCode.Combine(Shape, EndOffset, Mirrored);
    }
}
